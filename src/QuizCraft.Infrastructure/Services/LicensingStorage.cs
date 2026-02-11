using System.Text.Json;
using QuizCraft.Domain.Models;
using Serilog;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Armazena e recupera a licenca localmente usando DPAPI para protecao.
/// O arquivo e salvo em AppData/QuizCraft com criptografia vinculada ao usuario.
/// </summary>
public class LicensingStorage
{
    private static readonly ILogger Logger = Log.ForContext<LicensingStorage>();
    private readonly string _filePath;

    /// <summary>
    /// Inicializa o storage, definindo o caminho do arquivo de licenca.
    /// </summary>
    public LicensingStorage()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "QuizCraft");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "license.dat");
    }

    /// <summary>
    /// Salva o registro de licenca localmente, criptografando com DPAPI.
    /// </summary>
    /// <param name="record">Registro da licenca a ser salvo.</param>
    public void Save(LicenseRecord record)
    {
        try
        {
            var json = JsonSerializer.Serialize(record);
            var encrypted = CryptoHelper.Protect(json);
            File.WriteAllText(_filePath, encrypted);
            Logger.Information("Licenca salva localmente para {Email}", record.Email);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Erro ao salvar licenca localmente");
        }
    }

    /// <summary>
    /// Carrega o registro de licenca do arquivo local, descriptografando com DPAPI.
    /// </summary>
    /// <returns>Registro da licenca ou null se nao encontrado/invalido.</returns>
    public LicenseRecord? Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var encrypted = File.ReadAllText(_filePath);
            var json = CryptoHelper.Unprotect(encrypted);
            return JsonSerializer.Deserialize<LicenseRecord>(json);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Erro ao carregar licenca local (arquivo corrompido ou de outro usuario)");
            return null;
        }
    }

    /// <summary>
    /// Remove o arquivo de licenca local (logout/desativacao).
    /// </summary>
    public void Delete()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                Logger.Information("Licenca local removida");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Erro ao remover licenca local");
        }
    }

    /// <summary>
    /// Verifica se existe um arquivo de licenca salvo localmente.
    /// </summary>
    public bool Exists() => File.Exists(_filePath);
}
