using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Application.Services;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;
using System.Collections.ObjectModel;
using Wpf.Ui.Appearance;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de configurações. Gerencia tema, fonte, backup, importação e exportação de dados.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly QuizCraftDbContext _context;
    private readonly IBackupService _backupService;
    private readonly BackupSchedulerService _backupScheduler;
    private readonly ImportExportService _importExportService;

    /// <summary>Indica se o tema escuro está ativo.</summary>
    [ObservableProperty] private bool _isDarkTheme;
    /// <summary>Tamanho da fonte da interface.</summary>
    [ObservableProperty] private int _fontSize = 14;
    /// <summary>Intervalo em dias entre backups automáticos.</summary>
    [ObservableProperty] private int _backupIntervalDays = 15;
    /// <summary>Quantidade máxima de backups a manter.</summary>
    [ObservableProperty] private int _backupRetentionCount = 10;
    /// <summary>Diretório onde os backups são armazenados.</summary>
    [ObservableProperty] private string _backupDirectory = string.Empty;
    /// <summary>Mensagem de status exibida após operações.</summary>
    [ObservableProperty] private string? _statusMessage;
    /// <summary>Data do último backup realizado.</summary>
    [ObservableProperty] private string? _lastBackupDate;

    /// <summary>Lista de backups disponíveis.</summary>
    public ObservableCollection<BackupListItem> Backups { get; } = new();

    /// <summary>Inicializa o ViewModel com os serviços de backup, agendamento e importação/exportação.</summary>
    public SettingsViewModel(QuizCraftDbContext context, IBackupService backupService,
        BackupSchedulerService backupScheduler, ImportExportService importExportService)
    {
        _context = context;
        _backupService = backupService;
        _backupScheduler = backupScheduler;
        _importExportService = importExportService;
    }

    /// <summary>Carrega configurações e backups ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        await LoadSettingsAsync();
        await LoadBackupsAsync();
    }

    /// <summary>Carrega as configurações salvas do banco de dados.</summary>
    private async Task LoadSettingsAsync()
    {
        var settings = await _context.AppSettings.AsNoTracking().ToListAsync();

        IsDarkTheme = GetSetting(settings, "Theme") == "Dark";
        if (int.TryParse(GetSetting(settings, "FontSize"), out var fs)) FontSize = fs;
        if (int.TryParse(GetSetting(settings, "BackupIntervalDays"), out var bi)) BackupIntervalDays = bi;
        if (int.TryParse(GetSetting(settings, "BackupRetentionCount"), out var br)) BackupRetentionCount = br;
        BackupDirectory = await _backupService.GetBackupDirectoryAsync();
    }

    /// <summary>Salva todas as configurações no banco e atualiza o agendador de backup.</summary>
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await SaveSettingAsync("Theme", IsDarkTheme ? "Dark" : "Light");
            await SaveSettingAsync("FontSize", FontSize.ToString());
            await SaveSettingAsync("BackupIntervalDays", BackupIntervalDays.ToString());
            await SaveSettingAsync("BackupRetentionCount", BackupRetentionCount.ToString());

            _backupScheduler.Configure(BackupIntervalDays, BackupRetentionCount);
            StatusMessage = "Configurações salvas com sucesso!";
        });
    }

    /// <summary>Cria um backup manual e remove backups antigos conforme retenção.</summary>
    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var path = await _backupService.CreateBackupAsync();
            await _backupService.DeleteOldBackupsAsync(BackupRetentionCount);
            StatusMessage = $"Backup criado: {System.IO.Path.GetFileName(path)}";
            await LoadBackupsAsync();
        });
    }

    /// <summary>Restaura um backup selecionado pelo usuário.</summary>
    [RelayCommand]
    private async Task RestoreBackupAsync(BackupListItem? backup)
    {
        if (backup == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            await _backupService.RestoreBackupAsync(backup.FilePath);
            StatusMessage = "Backup restaurado com sucesso! Reinicie o aplicativo.";
        });
    }

    /// <summary>Exporta todas as questões em formato JSON para a Área de Trabalho.</summary>
    [RelayCommand]
    private async Task ExportQuestionsJsonAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var json = await _importExportService.ExportQuestionsJsonAsync();
            var path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"QuizCraft_Export_{DateTime.Now:yyyy-MM-dd}.json");
            await System.IO.File.WriteAllTextAsync(path, json);
            StatusMessage = $"Questões exportadas para: {path}";
        });
    }

    /// <summary>Exporta estatísticas em formato CSV para a Área de Trabalho.</summary>
    [RelayCommand]
    private async Task ExportStatsCsvAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var csv = await _importExportService.ExportStatsCsvAsync();
            var path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"QuizCraft_Stats_{DateTime.Now:yyyy-MM-dd}.csv");
            await System.IO.File.WriteAllTextAsync(path, csv);
            StatusMessage = $"Estatísticas exportadas para: {path}";
        });
    }

    /// <summary>Carrega a lista de backups existentes.</summary>
    private async Task LoadBackupsAsync()
    {
        var backups = await _backupService.GetBackupsAsync();
        Backups.Clear();
        foreach (var b in backups)
        {
            Backups.Add(new BackupListItem(b.FilePath, b.FileName, b.CreatedAt,
                $"{b.SizeBytes / 1024.0:F1} KB"));
        }

        if (Backups.Count > 0)
            LastBackupDate = Backups[0].CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }

    /// <summary>Salva ou atualiza uma configuração individual no banco.</summary>
    private async Task SaveSettingAsync(string key, string value)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting != null)
        {
            setting.Value = value;
        }
        else
        {
            _context.AppSettings.Add(new Domain.Entities.AppSettings { Key = key, Value = value });
        }
        await _context.SaveChangesAsync();
    }

    /// <summary>Aplica o tema imediatamente ao alterar a opção.</summary>
    partial void OnIsDarkThemeChanged(bool value)
    {
        ApplyTheme(value);
    }

    /// <summary>Aplica o tema claro ou escuro na interface WPF.</summary>
    public static void ApplyTheme(bool isDark)
    {
        ApplicationThemeManager.Apply(
            isDark ? ApplicationTheme.Dark : ApplicationTheme.Light);
    }

    /// <summary>Busca o valor de uma configuração por chave.</summary>
    private static string? GetSetting(List<Domain.Entities.AppSettings> settings, string key) =>
        settings.FirstOrDefault(s => s.Key == key)?.Value;
}

/// <summary>Item da lista de backups com caminho, nome, data e tamanho.</summary>
public record BackupListItem(string FilePath, string FileName, DateTime CreatedAt, string Size);
