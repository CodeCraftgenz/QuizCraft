using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de resultados do quiz. Exibe resumo de acertos, tempo, detalhes por questão e por tópico.
/// </summary>
public partial class ResultsViewModel : BaseViewModel
{
    private readonly IQuizSessionRepository _sessionRepository;

    /// <summary>Sessão do quiz com seus itens.</summary>
    [ObservableProperty] private QuizSession? _session;
    /// <summary>Taxa de acerto percentual.</summary>
    [ObservableProperty] private double _accuracyRate;
    /// <summary>Quantidade de respostas corretas.</summary>
    [ObservableProperty] private int _correctCount;
    /// <summary>Quantidade de respostas incorretas.</summary>
    [ObservableProperty] private int _incorrectCount;
    /// <summary>Total de questões do quiz.</summary>
    [ObservableProperty] private int _totalQuestions;
    /// <summary>Duração formatada da sessão (mm:ss).</summary>
    [ObservableProperty] private string _duration = "0:00";

    /// <summary>Lista detalhada dos resultados de cada questão.</summary>
    public ObservableCollection<QuizResultItem> ResultItems { get; } = new();
    /// <summary>Resumo de desempenho agrupado por tópico.</summary>
    public ObservableCollection<TopicResultSummary> TopicSummaries { get; } = new();

    /// <summary>Inicializa o ViewModel com o repositório de sessões.</summary>
    public ResultsViewModel(IQuizSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    /// <summary>Carrega os dados de uma sessão de quiz e calcula estatísticas.</summary>
    public async Task LoadSessionAsync(int sessionId)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            Session = await _sessionRepository.GetWithItemsAsync(sessionId);
            if (Session == null) return;

            // Calcula estatísticas gerais
            CorrectCount = Session.CorrectCount;
            TotalQuestions = Session.TotalQuestions;
            IncorrectCount = TotalQuestions - CorrectCount;
            AccuracyRate = TotalQuestions > 0 ? (double)CorrectCount / TotalQuestions * 100 : 0;

            var ts = TimeSpan.FromSeconds(Session.DurationSeconds);
            Duration = $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}";

            // Resultados individuais por questão
            ResultItems.Clear();
            foreach (var item in Session.Items.OrderBy(i => i.Order))
            {
                ResultItems.Add(new QuizResultItem(
                    item.Order + 1,
                    item.Question.Statement.Length > 80
                        ? item.Question.Statement[..80] + "..." : item.Question.Statement,
                    item.IsCorrect,
                    item.TimeSeconds,
                    item.MarkedForReview,
                    item.Question.Topic?.Name ?? "",
                    item.Question.Explanation
                ));
            }

            // Agrupa resultados por tópico para resumo
            TopicSummaries.Clear();
            var groups = Session.Items.GroupBy(i => i.Question.Topic?.Name ?? "Sem tópico");
            foreach (var g in groups.OrderBy(g => g.Key))
            {
                var total = g.Count();
                var correct = g.Count(i => i.IsCorrect);
                TopicSummaries.Add(new TopicResultSummary(
                    g.Key, correct, total, total > 0 ? (double)correct / total * 100 : 0));
            }
        });
    }

    /// <summary>Comando para revisar os erros (tratado pela navegação na MainWindow).</summary>
    [RelayCommand]
    private void ReviewErrors()
    {
        // Tratado pelo serviço de navegação na MainWindow
    }
}

/// <summary>Item de resultado individual de uma questão no quiz.</summary>
public record QuizResultItem(int Number, string Statement, bool IsCorrect, int TimeSeconds,
    bool MarkedForReview, string TopicName, string? Explanation);
/// <summary>Resumo de desempenho por tópico (acertos, total e taxa).</summary>
public record TopicResultSummary(string TopicName, int Correct, int Total, double AccuracyRate);
