using QuizCraft.Domain.Entities;

namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Serviço de repetição espaçada — gerencia o agendamento de revisões
/// com base no algoritmo de memorização por intervalos crescentes.
/// </summary>
public interface ISpacedRepetitionService
{
    /// <summary>Atualiza o nível de domínio de uma questão após uma tentativa.</summary>
    /// <param name="questionId">Identificador da questão.</param>
    /// <param name="isCorrect">Se a resposta foi correta.</param>
    /// <param name="timeSeconds">Tempo gasto na resposta em segundos.</param>
    Task UpdateMasteryAsync(int questionId, bool isCorrect, int timeSeconds);
    /// <summary>Retorna a fila de questões pendentes de revisão.</summary>
    /// <param name="subjectId">Filtro opcional por matéria.</param>
    /// <param name="count">Quantidade máxima de questões a retornar.</param>
    Task<IReadOnlyList<Question>> GetReviewQueueAsync(int? subjectId = null, int count = 20);
    /// <summary>Conta quantas questões estão pendentes de revisão.</summary>
    Task<int> GetDueCountAsync(int? subjectId = null);
    /// <summary>Calcula a data da próxima revisão com base no nível atual e no resultado.</summary>
    DateTime CalculateNextReview(int currentLevel, bool wasCorrect);
    /// <summary>Calcula o novo nível de domínio com base no nível atual e no resultado.</summary>
    int CalculateNewLevel(int currentLevel, bool wasCorrect);
}
