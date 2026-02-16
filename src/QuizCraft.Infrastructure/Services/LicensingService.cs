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
    /// Se a licenca local existe e o hardware confere, aceita direto sem consultar servidor.
    /// Validacao online ocorre apenas na ativacao inicial.
    /// </summary>
    public Task<LicenseState> CheckLicenseAsync()
    {
        // 1. Verificar se existe licenca salva localmente
        var stored = _storage.Load();
        if (stored == null)
        {
            Logger.Information("Nenhuma licenca encontrada localmente");
            return Task.FromResult(LicenseState.NotFound);
        }

        // 2. Verificar se o hardware confere
        if (stored.HardwareId != _hardwareId)
        {
            Logger.Warning("Hardware ID nao confere. Local: {Stored}, Atual: {Current}",
                stored.HardwareId, _hardwareId);
            _storage.Delete();
            return Task.FromResult(LicenseState.Invalid);
        }

        // 3. Licenca local existe e hardware confere - aceitar direto
        Logger.Information("Licenca valida localmente para {Email} (ativada em {Date})",
            stored.Email, stored.ActivatedAt.ToString("dd/MM/yyyy"));
        return Task.FromResult(LicenseState.Valid);
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

}
