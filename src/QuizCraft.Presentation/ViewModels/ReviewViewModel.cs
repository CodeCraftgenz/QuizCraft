using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de revisão espaçada. Exibe a fila de questões pendentes para revisão com base no algoritmo de repetição.
/// </summary>
public partial class ReviewViewModel : BaseViewModel
{
    private readonly ISpacedRepetitionService _spacedRepetitionService;
    private readonly QuizCraftDbContext _context;

    /// <summary>Quantidade de questões pendentes para hoje.</summary>
    [ObservableProperty] private int _dueToday;
    /// <summary>Quantidade de questões atrasadas (mais de 1 dia).</summary>
    [ObservableProperty] private int _overdue;
    /// <summary>Matéria selecionada como filtro.</summary>
    [ObservableProperty] private Subject? _filterSubject;

    /// <summary>Fila de questões para revisão.</summary>
    public ObservableCollection<ReviewQueueItem> ReviewQueue { get; } = new();
    /// <summary>Lista de matérias para filtro.</summary>
    public ObservableCollection<Subject> Subjects { get; } = new();

    /// <summary>Inicializa o ViewModel com o serviço de repetição espaçada e contexto.</summary>
    public ReviewViewModel(ISpacedRepetitionService spacedRepetitionService, QuizCraftDbContext context)
    {
        _spacedRepetitionService = spacedRepetitionService;
        _context = context;
    }

    /// <summary>Carrega matérias e fila de revisão ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        await LoadSubjectsAsync();
        await LoadReviewQueueAsync();
    }

    /// <summary>Carrega a lista de matérias para o filtro.</summary>
    private async Task LoadSubjectsAsync()
    {
        var subjects = await _context.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        Subjects.Clear();
        foreach (var s in subjects) Subjects.Add(s);
    }

    /// <summary>Carrega a fila de revisão com questões pendentes e atrasadas.</summary>
    [RelayCommand]
    private async Task LoadReviewQueueAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var dueCount = await _spacedRepetitionService.GetDueCountAsync(FilterSubject?.Id);
            DueToday = dueCount;

            var questions = await _spacedRepetitionService.GetReviewQueueAsync(FilterSubject?.Id, 50);
            var now = DateTime.UtcNow;

            ReviewQueue.Clear();
            foreach (var q in questions)
            {
                // Questão está atrasada se a revisão deveria ter sido feita há mais de 1 dia
                var isOverdue = q.Mastery != null && q.Mastery.NextReviewAt < now.AddDays(-1);
                ReviewQueue.Add(new ReviewQueueItem(
                    q.Id,
                    q.Statement.Length > 100 ? q.Statement[..100] + "..." : q.Statement,
                    q.Topic?.Name ?? "",
                    q.Topic?.Subject?.Name ?? "",
                    q.Mastery?.Level ?? 0,
                    q.Mastery?.NextReviewAt ?? now,
                    isOverdue
                ));
            }

            Overdue = ReviewQueue.Count(r => r.IsOverdue);
        });
    }

    /// <summary>Ao trocar o filtro de matéria, recarrega a fila de revisão.</summary>
    partial void OnFilterSubjectChanged(Subject? value)
    {
        _ = LoadReviewQueueAsync();
    }
}

/// <summary>Item da fila de revisão com dados da questão e nível de domínio.</summary>
public record ReviewQueueItem(int QuestionId, string Statement, string TopicName,
    string SubjectName, int MasteryLevel, DateTime NextReviewAt, bool IsOverdue);
