using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizCraft.Application.Services;

/// <summary>
/// Servico principal de quiz. Responsavel por montar questoes,
/// iniciar sessoes, registrar respostas e finalizar sessoes.
/// </summary>
public class QuizService
{
    private readonly QuizCraftDbContext _context;
    private readonly IQuestionRepository _questionRepository;
    private readonly ISpacedRepetitionService _spacedRepetitionService;
    private readonly IStatisticsService _statisticsService;

    /// <summary>
    /// Inicializa o servico de quiz com suas dependencias.
    /// </summary>
    public QuizService(
        QuizCraftDbContext context,
        IQuestionRepository questionRepository,
        ISpacedRepetitionService spacedRepetitionService,
        IStatisticsService statisticsService)
    {
        _context = context;
        _questionRepository = questionRepository;
        _spacedRepetitionService = spacedRepetitionService;
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Monta a lista de questoes para o quiz conforme o modo e os filtros informados.
    /// Suporta modos: revisao de erros, revisao espacada e quiz normal.
    /// </summary>
    public async Task<IReadOnlyList<Question>> BuildQuizQuestionsAsync(
        QuizMode mode, int? subjectId, int? topicId,
        List<int>? tagIds, int? minDifficulty, int? maxDifficulty,
        int count, bool randomize, bool shuffleChoices)
    {
        IReadOnlyList<Question> questions;

        if (mode == QuizMode.ErrorReview)
        {
            // Modo revisao de erros: busca questoes com nivel de dominio < 3
            questions = await _questionRepository.GetForQuizAsync(
                subjectId, topicId, tagIds, minDifficulty, maxDifficulty, count * 2, false);
            questions = questions
                .Where(q => q.Mastery == null || q.Mastery.Level < 3)
                .Take(count)
                .ToList();
        }
        else if (mode == QuizMode.SpacedReview)
        {
            questions = await _spacedRepetitionService.GetReviewQueueAsync(subjectId, count);
        }
        else
        {
            questions = await _questionRepository.GetForQuizAsync(
                subjectId, topicId, tagIds, minDifficulty, maxDifficulty, count, randomize);
        }

        // Embaralha as alternativas de cada questao se solicitado
        if (shuffleChoices)
        {
            var rng = new Random();
            foreach (var q in questions)
            {
                var shuffled = q.Choices.OrderBy(_ => rng.Next()).ToList();
                for (int i = 0; i < shuffled.Count; i++)
                    shuffled[i].Order = i;
                q.Choices = shuffled;
            }
        }

        return questions;
    }

    /// <summary>
    /// Inicia uma nova sessao de quiz e persiste no banco de dados.
    /// </summary>
    /// <param name="mode">Modo do quiz (normal, revisao de erros, revisao espacada).</param>
    /// <param name="totalQuestions">Total de questoes na sessao.</param>
    /// <param name="timeLimitSeconds">Limite de tempo em segundos (opcional).</param>
    /// <param name="filtersJson">JSON com os filtros aplicados (opcional).</param>
    public async Task<QuizSession> StartSessionAsync(QuizMode mode, int totalQuestions,
        int? timeLimitSeconds, string? filtersJson)
    {
        var session = new QuizSession
        {
            Mode = mode,
            TotalQuestions = totalQuestions,
            TimeLimitSeconds = timeLimitSeconds,
            FiltersJson = filtersJson,
            Status = SessionStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };

        _context.QuizSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    /// <summary>
    /// Registra a resposta de uma questao na sessao e atualiza o dominio via repeticao espacada.
    /// </summary>
    public async Task RecordAnswerAsync(int sessionId, int questionId, string? selectedAnswerJson,
        bool isCorrect, int timeSeconds, bool markedForReview, int order)
    {
        var item = new QuizSessionItem
        {
            SessionId = sessionId,
            QuestionId = questionId,
            SelectedAnswerJson = selectedAnswerJson,
            IsCorrect = isCorrect,
            TimeSeconds = timeSeconds,
            MarkedForReview = markedForReview,
            Order = order
        };

        _context.QuizSessionItems.Add(item);
        await _context.SaveChangesAsync();

        // Atualiza o dominio da questao (repeticao espacada)
        await _spacedRepetitionService.UpdateMasteryAsync(questionId, isCorrect, timeSeconds);
    }

    /// <summary>
    /// Finaliza a sessao de quiz, calculando estatisticas e registrando o dia de estudo.
    /// </summary>
    /// <param name="sessionId">ID da sessao a ser finalizada.</param>
    public async Task<QuizSession> FinishSessionAsync(int sessionId)
    {
        var session = await _context.QuizSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new InvalidOperationException($"Session {sessionId} not found.");

        // Marca sessao como concluida e calcula acertos e duracao
        session.Status = SessionStatus.Completed;
        session.EndedAt = DateTime.UtcNow;
        session.CorrectCount = session.Items.Count(i => i.IsCorrect);
        session.DurationSeconds = (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds;

        await _context.SaveChangesAsync();

        // Registra o dia de estudo para calculo de sequencia (streak)
        await _statisticsService.RecordStudyDayAsync(
            session.Items.Count,
            session.CorrectCount,
            session.DurationSeconds);

        return session;
    }
}
