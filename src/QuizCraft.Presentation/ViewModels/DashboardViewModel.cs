using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizCraft.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel do painel principal (Dashboard). Exibe estatísticas gerais, gráficos de desempenho e tópicos fracos.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly IStatisticsService _statisticsService;
    private readonly ISpacedRepetitionService _spacedRepetitionService;

    /// <summary>Total de questões cadastradas no sistema.</summary>
    [ObservableProperty] private int _totalQuestions;
    /// <summary>Quantidade de questões já estudadas pelo usuário.</summary>
    [ObservableProperty] private int _questionsStudied;
    /// <summary>Taxa de acerto nos últimos 7 dias.</summary>
    [ObservableProperty] private double _accuracyRate7Days;
    /// <summary>Taxa de acerto nos últimos 30 dias.</summary>
    [ObservableProperty] private double _accuracyRate30Days;
    /// <summary>Tempo médio de resposta por questão em segundos.</summary>
    [ObservableProperty] private double _averageTimeSeconds;
    /// <summary>Sequência atual de dias consecutivos de estudo.</summary>
    [ObservableProperty] private int _currentStreak;
    /// <summary>Total de sessões de quiz realizadas.</summary>
    [ObservableProperty] private int _totalSessions;
    /// <summary>Quantidade de questões pendentes para revisão espaçada.</summary>
    [ObservableProperty] private int _dueForReview;

    /// <summary>Barras do gráfico de acerto diário.</summary>
    public ObservableCollection<ChartBarItem> DailyAccuracyBars { get; } = new();
    /// <summary>Barras do gráfico de desempenho por tópico.</summary>
    public ObservableCollection<ChartBarItem> TopicBars { get; } = new();
    /// <summary>Lista dos tópicos com pior desempenho.</summary>
    public ObservableCollection<WeakTopicItem> WeakTopics { get; } = new();

    /// <summary>
    /// Inicializa o ViewModel com os serviços de estatísticas e repetição espaçada.
    /// </summary>
    public DashboardViewModel(IStatisticsService statisticsService, ISpacedRepetitionService spacedRepetitionService)
    {
        _statisticsService = statisticsService;
        _spacedRepetitionService = spacedRepetitionService;
    }

    /// <summary>Carrega todos os dados do dashboard ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await LoadDashboardStatsAsync();
            await LoadChartsAsync();
            await LoadWeakTopicsAsync();
        });
    }

    /// <summary>Comando para atualizar todos os dados do dashboard.</summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await InitializeAsync();
    }

    /// <summary>Carrega as estatísticas resumidas do dashboard.</summary>
    private async Task LoadDashboardStatsAsync()
    {
        var stats = await _statisticsService.GetDashboardStatsAsync();
        TotalQuestions = stats.TotalQuestions;
        QuestionsStudied = stats.QuestionsStudied;
        AccuracyRate7Days = stats.AccuracyRate7Days;
        AccuracyRate30Days = stats.AccuracyRate30Days;
        AverageTimeSeconds = stats.AverageTimeSeconds;
        CurrentStreak = stats.CurrentStreak;
        TotalSessions = stats.TotalSessions;
        DueForReview = stats.DueForReview;
    }

    /// <summary>Carrega os dados dos gráficos de desempenho diário e por tópico.</summary>
    private async Task LoadChartsAsync()
    {
        // Barras de acerto diário dos últimos 14 dias
        var daily = await _statisticsService.GetDailyPerformanceAsync(14);
        DailyAccuracyBars.Clear();
        foreach (var d in daily)
        {
            DailyAccuracyBars.Add(new ChartBarItem(d.Date.ToString("dd/MM"), d.AccuracyRate, d.QuestionsAnswered));
        }

        // Barras de desempenho por tópico (máximo 8)
        var topics = await _statisticsService.GetPerformanceByTopicAsync();
        TopicBars.Clear();
        foreach (var t in topics.Take(8))
        {
            // Trunca nomes longos para caber no gráfico
            var label = t.TopicName.Length > 18 ? t.TopicName[..18] + "..." : t.TopicName;
            TopicBars.Add(new ChartBarItem(label, t.AccuracyRate, t.TotalAttempts));
        }
    }

    /// <summary>Carrega os 5 tópicos com pior desempenho para destaque.</summary>
    private async Task LoadWeakTopicsAsync()
    {
        var weak = await _statisticsService.GetWeakestTopicsAsync(5);
        WeakTopics.Clear();
        foreach (var t in weak)
        {
            WeakTopics.Add(new WeakTopicItem(t.TopicName, t.SubjectName, t.AccuracyRate, t.TotalAttempts));
        }
    }
}

/// <summary>Item de barra de gráfico com rótulo, valor percentual e contagem.</summary>
public record ChartBarItem(string Label, double Value, int Count);
/// <summary>Item de tópico fraco com nome, matéria, taxa de acerto e total de tentativas.</summary>
public record WeakTopicItem(string TopicName, string SubjectName, double AccuracyRate, int TotalAttempts);
