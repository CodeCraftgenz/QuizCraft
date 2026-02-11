using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Application.Services;

/// <summary>
/// Servico responsavel pelo algoritmo de repeticao espacada, inspirado no SM-2.
/// Gerencia o nivel de dominio de cada questao e calcula a proxima data de revisao.
/// </summary>
public class SpacedRepetitionService : ISpacedRepetitionService
{
    private readonly QuizCraftDbContext _context;
    private readonly IQuestionRepository _questionRepository;

    // Intervalos em dias para cada nivel de dominio (0=novo, 1=iniciante, ..., 5=dominado)
    private static readonly double[] ReviewIntervals = [0, 1, 3, 7, 14, 30];

    /// <summary>
    /// Inicializa o servico com o contexto do banco e o repositorio de questoes.
    /// </summary>
    public SpacedRepetitionService(QuizCraftDbContext context, IQuestionRepository questionRepository)
    {
        _context = context;
        _questionRepository = questionRepository;
    }

    /// <summary>
    /// Atualiza o nivel de dominio de uma questao apos o aluno responder.
    /// Cria o registro de dominio se ainda nao existir.
    /// </summary>
    /// <param name="questionId">ID da questao respondida.</param>
    /// <param name="isCorrect">Indica se a resposta foi correta.</param>
    /// <param name="timeSeconds">Tempo gasto na resposta, em segundos.</param>
    public async Task UpdateMasteryAsync(int questionId, bool isCorrect, int timeSeconds)
    {
        var mastery = await _context.Masteries.FirstOrDefaultAsync(m => m.QuestionId == questionId);

        // Cria registro de dominio caso seja a primeira tentativa nesta questao
        if (mastery == null)
        {
            mastery = new Mastery
            {
                QuestionId = questionId,
                Level = 0,
                TotalAttempts = 0,
                TotalCorrect = 0
            };
            _context.Masteries.Add(mastery);
        }

        mastery.TotalAttempts++;
        mastery.LastAttemptAt = DateTime.UtcNow;

        if (isCorrect)
        {
            // Acertou: incrementa sequencia de acertos e sobe o nivel
            mastery.TotalCorrect++;
            mastery.RightStreak++;
            mastery.WrongStreak = 0;
            mastery.Level = CalculateNewLevel(mastery.Level, true);
        }
        else
        {
            // Errou: incrementa sequencia de erros e reduz o nivel
            mastery.WrongStreak++;
            mastery.RightStreak = 0;
            mastery.Level = CalculateNewLevel(mastery.Level, false);
        }

        // Calcula proxima data de revisao com base no nivel atual
        mastery.NextReviewAt = CalculateNextReview(mastery.Level, isCorrect);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retorna a fila de questoes pendentes de revisao (vencidas ou no prazo).
    /// </summary>
    /// <param name="subjectId">Filtro opcional por disciplina.</param>
    /// <param name="count">Quantidade maxima de questoes a retornar.</param>
    public async Task<IReadOnlyList<Question>> GetReviewQueueAsync(int? subjectId = null, int count = 20)
    {
        return await _questionRepository.GetDueForReviewAsync(DateTime.UtcNow, subjectId, count);
    }

    /// <summary>
    /// Retorna a quantidade de questoes pendentes de revisao.
    /// </summary>
    public async Task<int> GetDueCountAsync(int? subjectId = null)
    {
        var query = _context.Masteries
            .Where(m => m.NextReviewAt <= DateTime.UtcNow);

        if (subjectId.HasValue)
            query = query.Where(m => m.Question.Topic.SubjectId == subjectId.Value);

        return await query.CountAsync();
    }

    /// <summary>
    /// Calcula a proxima data de revisao com base no nivel de dominio e se acertou ou errou.
    /// </summary>
    public DateTime CalculateNextReview(int currentLevel, bool wasCorrect)
    {
        var level = Math.Clamp(currentLevel, 0, ReviewIntervals.Length - 1);
        var intervalDays = ReviewIntervals[level];

        // Se errou, reduz o intervalo pela metade para revisar mais cedo
        if (!wasCorrect)
            intervalDays = Math.Max(0.5, intervalDays * 0.5);

        return DateTime.UtcNow.AddDays(intervalDays);
    }

    /// <summary>
    /// Calcula o novo nivel de dominio: sobe 1 se acertou, desce 2 se errou.
    /// Nivel varia entre 0 (novo) e 5 (dominado).
    /// </summary>
    public int CalculateNewLevel(int currentLevel, bool wasCorrect)
    {
        if (wasCorrect)
            return Math.Min(currentLevel + 1, 5); // Sobe 1 nivel, maximo 5
        else
            return Math.Max(currentLevel - 2, 0); // Desce 2 niveis, minimo 0
    }
}
