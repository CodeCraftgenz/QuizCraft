using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizCraft.Application.Services;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Domain.Models;
using QuizCraft.Infrastructure.Data;
using QuizCraft.Infrastructure.Repositories;
using QuizCraft.Infrastructure.Services;
using QuizCraft.Presentation.ViewModels;
using Serilog;

namespace QuizCraft.Presentation;

/// <summary>
/// Ponto de entrada do aplicativo WPF. Configura logging, injecao de dependencia,
/// banco de dados, licenciamento e tema.
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>Provedor de servicos (DI) global da aplicacao.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Inicializacao do aplicativo: configura Serilog, DI, banco, licenciamento, tema e backup.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configura o Serilog para logging em arquivo com rotacao diaria
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                System.IO.Path.Combine(DatabaseInitializer.GetLogsPath(), "quizcraft-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        Log.Information("QuizCraft starting...");

        // Configura a injecao de dependencia
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Inicializa o banco de dados SQLite e aplica seed de dados
        try
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QuizCraftDbContext>();
            context.Database.EnsureCreated();
            Log.Information("Database initialized at {Path}", DatabaseInitializer.GetDatabasePath());

            // Popular banco com dados de exemplo (so se estiver vazio)
            DatabaseSeeder.SeedAsync(context).GetAwaiter().GetResult();
            Log.Information("Seed de dados verificado/aplicado.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to initialize database");
            MessageBox.Show($"Erro ao inicializar banco de dados:\n{ex.Message}",
                "QuizCraft - Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        // Verificar licenca antes de abrir o app
        if (!CheckLicenseOnStartup())
        {
            Log.Information("Licenca nao ativada. Encerrando...");
            Shutdown(0);
            return;
        }

        // Aplica o tema salvo nas configuracoes
        try
        {
            using var themeScope = Services.CreateScope();
            var themeContext = themeScope.ServiceProvider.GetRequiredService<QuizCraftDbContext>();
            var themeSetting = themeContext.AppSettings.FirstOrDefault(s => s.Key == "Theme");
            if (themeSetting?.Value == "Dark")
                SettingsViewModel.ApplyTheme(true);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load theme setting");
        }

        // Inicia o agendador de backup automatico
        var scheduler = Services.GetRequiredService<BackupSchedulerService>();
        scheduler.Start();

        // Exibe a janela principal e configura encerramento ao fechar
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Closed += (_, _) => Shutdown();
        mainWindow.Show();
    }

    /// <summary>
    /// Verifica a licenca no startup. Exibe a janela de ativacao se necessario.
    /// Retorna true se a licenca e valida, false se o usuario fechou sem ativar.
    /// </summary>
    private bool CheckLicenseOnStartup()
    {
        var licensingService = Services.GetRequiredService<ILicensingService>();

        // Verificar licenca existente
        var state = licensingService.CheckLicenseAsync().GetAwaiter().GetResult();
        Log.Information("Estado da licenca: {State}", state);

        if (state == LicenseState.Valid)
            return true;

        // Licenca nao encontrada ou invalida - exibir janela de ativacao
        var viewModel = Services.GetRequiredService<LicenseViewModel>();
        var licenseWindow = new LicenseWindow(viewModel);
        var result = licenseWindow.ShowDialog();

        return result == true && licenseWindow.IsActivated;
    }

    /// <summary>Encerramento do aplicativo: libera recursos e fecha o log.</summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("QuizCraft shutting down...");

        if (Services is IDisposable disposable)
            disposable.Dispose();

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    /// <summary>
    /// Configura todos os servicos no container de injecao de dependencia.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Banco de dados SQLite
        var dbPath = DatabaseInitializer.GetDatabasePath();
        services.AddDbContext<QuizCraftDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"),
            ServiceLifetime.Transient);

        // Repositorios de acesso a dados
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddTransient<IQuestionRepository, QuestionRepository>();
        services.AddTransient<IQuizSessionRepository, QuizSessionRepository>();

        // Servicos de aplicacao
        services.AddTransient<IBackupService, BackupService>();
        services.AddTransient<IStatisticsService, StatisticsService>();
        services.AddTransient<ISpacedRepetitionService, SpacedRepetitionService>();
        services.AddTransient<QuizService>();
        services.AddTransient<ImportExportService>();
        services.AddSingleton<BackupSchedulerService>();

        // Licenciamento
        services.AddSingleton<ILicensingService, LicensingService>();

        // ViewModels da camada de apresentacao
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SubjectsViewModel>();
        services.AddTransient<QuestionsViewModel>();
        services.AddTransient<ReviewViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HelpViewModel>();
        services.AddTransient<LicenseViewModel>();

        // Janela principal (singleton)
        services.AddSingleton<MainWindow>();
    }
}
