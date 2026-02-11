using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de histórico de sessões. Exibe sessões anteriores com estatísticas e permite visualizar detalhes.
/// </summary>
public partial class HistoryViewModel : BaseViewModel
{
    private readonly IQuizSessionRepository _sessionRepository;
    /// <summary>Callback para navegar à tela de resultados de uma sessão.</summary>
    private readonly Action<int> _onViewSession;

    /// <summary>Sessão selecionada na lista.</summary>
    [ObservableProperty] private QuizSession? _selectedSession;

    /// <summary>Lista de sessões do histórico.</summary>
    public ObservableCollection<SessionHistoryItem> Sessions { get; } = new();

    /// <summary>Inicializa o ViewModel com o repositório e callback de navegação.</summary>
    public HistoryViewModel(IQuizSessionRepository sessionRepository, Action<int> onViewSession)
    {
        _sessionRepository = sessionRepository;
        _onViewSession = onViewSession;
    }

    /// <summary>Carrega o histórico de sessões ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        await LoadSessionsAsync();
    }

    /// <summary>Carrega as 50 sessões mais recentes com estatísticas calculadas.</summary>
    [RelayCommand]
    private async Task LoadSessionsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var sessions = await _sessionRepository.GetRecentSessionsAsync(50);
            Sessions.Clear();
            foreach (var s in sessions)
            {
                var accuracy = s.TotalQuestions > 0 ? (double)s.CorrectCount / s.TotalQuestions * 100 : 0;
                var duration = TimeSpan.FromSeconds(s.DurationSeconds);
                Sessions.Add(new SessionHistoryItem(
                    s.Id,
                    s.StartedAt,
                    ModeToPortuguese(s.Mode),
                    s.TotalQuestions,
                    s.CorrectCount,
                    accuracy,
                    $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}"
                ));
            }
        });
    }

    /// <summary>Converte o modo do quiz para texto em português.</summary>
    private static string ModeToPortuguese(QuizMode mode) => mode switch
    {
        QuizMode.Training => "Treino",
        QuizMode.Exam => "Prova",
        QuizMode.ErrorReview => "Revisão de Erros",
        QuizMode.SpacedReview => "Revisão Espaçada",
        _ => mode.ToString()
    };

    /// <summary>Navega para a tela de resultados da sessão selecionada.</summary>
    [RelayCommand]
    private void ViewSession(SessionHistoryItem? item)
    {
        if (item != null)
            _onViewSession(item.SessionId);
    }
}

/// <summary>Item do histórico com dados resumidos de uma sessão de quiz.</summary>
public record SessionHistoryItem(int SessionId, DateTime Date, string Mode,
    int TotalQuestions, int CorrectCount, double AccuracyRate, string Duration);
