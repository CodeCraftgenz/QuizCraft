using QuizCraft.Domain.Interfaces;
using QuizCraft.Domain.Models;
using Serilog;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Implementacao do servico de licenciamento.
/// Gerencia ativacao, validacao online/offline e armazenamento local da licenca.
/// Usa DPAPI para proteger os dados locais e WMI para fingerprint do hardware.
/// </summary>
public class LicensingService : ILicensingService
{
    private static readonly ILogger Logger = Log.ForContext<LicensingService>();

    /// <summary>Periodo maximo para aceitar licenca offline (7 dias).</summary>
    private static readonly TimeSpan OfflineGracePeriod = TimeSpan.FromDays(7);

    private readonly LicenseApiClient _apiClient;
    private readonly LicensingStorage _storage;
    private readonly string _hardwareId;

    /// <summary>
    /// Inicializa o servico com o cliente da API e o storage local.
    /// </summary>
    public LicensingService()
    {
        _apiClient = new LicenseApiClient();
        _storage = new LicensingStorage();
        _hardwareId = HardwareHelper.GetHardwareId();
    }

    /// <summary>
    /// Verifica se existe uma licenca valida.
    /// Se a licenca local existe e o hardware confere, aceita direto.
    /// Validacao online e feita apenas periodicamente (a cada 7 dias).
    /// </summary>
    public async Task<LicenseState> CheckLicenseAsync()
    {
        // 1. Verificar se existe licenca salva localmente
        var stored = _storage.Load();
        if (stored == null)
        {
            Logger.Information("Nenhuma licenca encontrada localmente");
            return LicenseState.NotFound;
        }

        // 2. Verificar se o hardware confere
        if (stored.HardwareId != _hardwareId)
        {
            Logger.Warning("Hardware ID nao confere. Local: {Stored}, Atual: {Current}",
                stored.HardwareId, _hardwareId);
            _storage.Delete();
            return LicenseState.Invalid;
        }

        // 3. Se a ultima validacao online foi recente, aceitar sem consultar o servidor
        var timeSinceLastValidation = DateTime.UtcNow - stored.LastValidatedAt;
        if (timeSinceLastValidation < OfflineGracePeriod)
        {
            Logger.Information("Licenca aceita localmente (ultima validacao: {Hours}h atras)",
                timeSinceLastValidation.TotalHours.ToString("F1"));
            return LicenseState.Valid;
        }

        // 4. Validacao online periodica (a cada 7 dias)
        Logger.Information("Validacao online necessaria (ultima: {Days} dias atras)",
            timeSinceLastValidation.TotalDays.ToString("F0"));

        var result = await _apiClient.VerifyLicenseAsync(stored.Email, _hardwareId);

        if (result.Valid)
        {
            stored.LastValidatedAt = DateTime.UtcNow;
            _storage.Save(stored);
            Logger.Information("Licenca revalidada online para {Email}", stored.Email);
            return LicenseState.Valid;
        }

        // 5. Se falhou por conexao, dar mais tempo de graca (30 dias no total)
        if (IsOfflineAcceptable(result.Message))
        {
            if (timeSinceLastValidation < TimeSpan.FromDays(30))
            {
                Logger.Information("Sem conexao, mas dentro do periodo estendido de graca ({Days} dias)",
                    timeSinceLastValidation.TotalDays.ToString("F0"));
                return LicenseState.Valid;
            }

            Logger.Warning("Periodo de graca offline expirado ({Days} dias)", timeSinceLastValidation.TotalDays);
            return LicenseState.Error;
        }

        // 6. Licenca invalida no servidor
        Logger.Warning("Licenca rejeitada pelo servidor: {Message}", result.Message);
        return LicenseState.Invalid;
    }

    /// <summary>
    /// Ativa o dispositivo com o email informado.
    /// Envia requisicao ao servidor e salva a licenca localmente.
    /// </summary>
    public async Task<DeviceActivationResult> ActivateAsync(string email)
    {
        var result = await _apiClient.ActivateDeviceAsync(email, _hardwareId);

        if (result.Success && !string.IsNullOrEmpty(result.LicenseKey))
        {
            // Salvar licenca localmente
            var record = new LicenseRecord
            {
                Email = email.Trim().ToLowerInvariant(),
                LicenseKey = result.LicenseKey,
                HardwareId = _hardwareId,
                ActivatedAt = DateTime.UtcNow,
                LastValidatedAt = DateTime.UtcNow
            };
            _storage.Save(record);
            Logger.Information("Dispositivo ativado com sucesso para {Email}", email);
        }

        return result;
    }

    /// <summary>
    /// Remove a licenca salva localmente (logout).
    /// </summary>
    public void RemoveLicense()
    {
        _storage.Delete();
        Logger.Information("Licenca removida pelo usuario");
    }

    /// <summary>
    /// Retorna o registro da licenca armazenada localmente.
    /// </summary>
    public LicenseRecord? GetStoredLicense() => _storage.Load();

    /// <summary>
    /// Verifica se a mensagem de erro indica problema de conexao (aceitavel para modo offline).
    /// </summary>
    private static bool IsOfflineAcceptable(string message)
    {
        return message.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("conexao", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("offline", StringComparison.OrdinalIgnoreCase);
    }
}
