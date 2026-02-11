using QuizCraft.Domain.Interfaces;
using Serilog;

namespace QuizCraft.Application.Services;

/// <summary>
/// Agendador de backup automatico. Executa backups periodicamente
/// e remove copias antigas conforme a politica de retencao.
/// </summary>
public class BackupSchedulerService : IDisposable
{
    private readonly IBackupService _backupService;
    private readonly ILogger _logger;
    private Timer? _timer;
    private int _intervalDays = 15;   // Intervalo padrao entre backups (dias)
    private int _retentionCount = 10; // Quantidade maxima de backups mantidos

    /// <summary>
    /// Inicializa o agendador com o servico de backup.
    /// </summary>
    public BackupSchedulerService(IBackupService backupService)
    {
        _backupService = backupService;
        _logger = Log.ForContext<BackupSchedulerService>();
    }

    /// <summary>
    /// Configura o intervalo entre backups e a quantidade maxima de copias retidas.
    /// </summary>
    public void Configure(int intervalDays, int retentionCount)
    {
        _intervalDays = Math.Max(1, intervalDays);    // Minimo 1 dia
        _retentionCount = Math.Max(1, retentionCount); // Minimo 1 backup
    }

    /// <summary>
    /// Inicia o timer periodico para execucao automatica de backups.
    /// </summary>
    public void Start()
    {
        var interval = TimeSpan.FromDays(_intervalDays);
        // Configura timer com atraso inicial igual ao intervalo (primeiro backup apos N dias)
        _timer = new Timer(async _ => await ExecuteBackupAsync(), null, interval, interval);
        _logger.Information("Backup scheduler started with interval of {Days} days", _intervalDays);
    }

    /// <summary>
    /// Para o agendador e libera o timer.
    /// </summary>
    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
        _logger.Information("Backup scheduler stopped");
    }

    /// <summary>
    /// Executa o backup manualmente ou via timer. Cria o backup e remove copias antigas.
    /// </summary>
    public async Task ExecuteBackupAsync()
    {
        try
        {
            _logger.Information("Starting scheduled backup...");
            var path = await _backupService.CreateBackupAsync();
            // Remove backups antigos que excedem o limite de retencao
            await _backupService.DeleteOldBackupsAsync(_retentionCount);
            _logger.Information("Scheduled backup completed: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Scheduled backup failed");
        }
    }

    /// <summary>
    /// Libera os recursos do timer ao descartar o servico.
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
    }
}
