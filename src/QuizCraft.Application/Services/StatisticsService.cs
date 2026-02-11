using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Application.Services;

/// <summary>
/// Servico de estatisticas do dashboard. Calcula metricas de desempenho,
/// sequencia de estudo, topicos fracos e desempenho diario.
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly QuizCraftDbContext _context;

    /// <summary>
    /// Inicializa o servico com o contexto do banco de dados.
    /// </summary>
    public StatisticsService(QuizCraftDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna as estatisticas consolidadas do dashboard do usuario.
    /// </summary>
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalQuestions = await _context.Questions.CountAsync();

        // Conta questoes distintas ja respondidas
        var questionsStudied = await _context.QuizSessionItems
            .Select(i => i.QuestionId)
            .Distinct()
            .CountAsync();

        // Calcula taxa de acerto dos ultimos 7 e 30 dias
        var now = DateTime.UtcNow;
        var accuracy7d = await CalculateAccuracyAsync(now.AddDays(-7), now);
        var accuracy30d = await CalculateAccuracyAsync(now.AddDays(-30), now);

        // Tempo medio por questao em sessoes concluidas
        var avgTime = await _context.QuizSessionItems
            .Where(i => i.Session.Status == SessionStatus.Completed)
            .AverageAsync(i => (double?)i.TimeSeconds) ?? 0;

        var streak = await CalculateStreakAsync();
        var totalSessions = await _context.QuizSessions
            .CountAsync(s => s.Status == SessionStatus.Completed);

        // Questoes pendentes de revisao (repeticao espacada)
        var dueForReview = await _context.Masteries
            .CountAsync(m => m.NextReviewAt <= now);

        return new DashboardStats(
            totalQuestions, questionsStudied, accuracy7d, accuracy30d,
            avgTime, streak, totalSessions, dueForReview
        );
    }

    /// <summary>
    /// Retorna os topicos com pior desempenho (minimo 3 tentativas).
    /// </summary>
    /// <param name="count">Quantidade de topicos fracos a retornar.</param>
    public async Task<IReadOnlyList<TopicPerformance>> GetWeakestTopicsAsync(int count = 5)
    {
        var performances = await GetPerformanceByTopicAsync();
        return performances
            .Where(p => p.TotalAttempts >= 3)
            .OrderBy(p => p.AccuracyRate)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Retorna o desempenho diario (taxa de acerto e total) dos ultimos N dias.
    /// </summary>
    /// <param name="days">Quantidade de dias para consultar.</param>
    public async Task<IReadOnlyList<DailyPerformance>> GetDailyPerformanceAsync(int days = 30)
    {
        var from = DateTime.UtcNow.Date.AddDays(-days);

        var raw = await _context.QuizSessionItems
            .Where(i => i.Session.StartedAt >= from && i.Session.Status == SessionStatus.Completed)
            .Select(i => new { i.Session.StartedAt, i.IsCorrect })
            .ToListAsync();

        // Agrupa por data e calcula taxa de acerto de cada dia
        var items = raw
            .GroupBy(i => i.StartedAt.Date)
            .Select(g => new DailyPerformance(
                g.Key,
                g.Count() == 0 ? 0 : (double)g.Count(i => i.IsCorrect) / g.Count() * 100,
                g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToList();

        return items;
    }

    /// <summary>
    /// Retorna o desempenho agrupado por topico, com filtro opcional por disciplina.
    /// </summary>
    public async Task<IReadOnlyList<TopicPerformance>> GetPerformanceByTopicAsync(int? subjectId = null)
    {
        var query = _context.QuizSessionItems
            .Include(i => i.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Subject)
            .Where(i => i.Session.Status == SessionStatus.Completed)
            .AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(i => i.Question.Topic.SubjectId == subjectId.Value);

        var raw = await query.ToListAsync();

        var result = raw
            .GroupBy(i => new { i.Question.Topic.Name, SubjectName = i.Question.Topic.Subject.Name })
            .Select(g => new TopicPerformance(
                g.Key.Name,
                g.Key.SubjectName,
                g.Count() == 0 ? 0 : (double)g.Count(i => i.IsCorrect) / g.Count() * 100,
                g.Count()
            ))
            .OrderBy(p => p.AccuracyRate)
            .ToList();

        return result;
    }

    /// <summary>
    /// Registra ou atualiza o dia de estudo atual para calculo de sequencia (streak).
    /// </summary>
    public async Task RecordStudyDayAsync(int questionsAnswered, int correctAnswers, int timeSeconds)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await _context.StudyStreaks.FirstOrDefaultAsync(s => s.Date == today);

        // Se ja existe registro do dia, acumula os valores
        if (existing != null)
        {
            existing.QuestionsAnswered += questionsAnswered;
            existing.CorrectAnswers += correctAnswers;
            existing.StudyTimeSeconds += timeSeconds;
        }
        else
        {
            _context.StudyStreaks.Add(new Domain.Entities.StudyStreak
            {
                Date = today,
                QuestionsAnswered = questionsAnswered,
                CorrectAnswers = correctAnswers,
                StudyTimeSeconds = timeSeconds
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calcula a taxa de acerto (%) em um periodo de datas.
    /// </summary>
    private async Task<double> CalculateAccuracyAsync(DateTime from, DateTime to)
    {
        var items = await _context.QuizSessionItems
            .Where(i => i.Session.StartedAt >= from && i.Session.StartedAt <= to
                && i.Session.Status == SessionStatus.Completed)
            .ToListAsync();

        if (items.Count == 0) return 0;
        return (double)items.Count(i => i.IsCorrect) / items.Count * 100;
    }

    /// <summary>
    /// Calcula a sequencia de dias consecutivos de estudo (streak).
    /// </summary>
    private async Task<int> CalculateStreakAsync()
    {
        var streaks = await _context.StudyStreaks
            .OrderByDescending(s => s.Date)
            .Take(60)
            .ToListAsync();

        if (streaks.Count == 0) return 0;

        var streak = 0;
        var expectedDate = DateTime.UtcNow.Date;

        // Percorre dias ordenados do mais recente ao mais antigo, verificando consecutividade
        foreach (var day in streaks)
        {
            if (day.Date == expectedDate || day.Date == expectedDate.AddDays(-1))
            {
                streak++;
                expectedDate = day.Date.AddDays(-1);
            }
            else break; // Quebra na sequencia: interrompe contagem
        }

        return streak;
    }
}
