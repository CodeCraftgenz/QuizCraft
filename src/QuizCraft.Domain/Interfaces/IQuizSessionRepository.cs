using QuizCraft.Domain.Entities;

namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Repositório especializado para sessões de quiz.
/// </summary>
public interface IQuizSessionRepository : IRepository<QuizSession>
{
    /// <summary>Obtém uma sessão com todos os seus itens (respostas) carregados.</summary>
    Task<QuizSession?> GetWithItemsAsync(int id);
    /// <summary>Retorna as sessões mais recentes, ordenadas por data de início.</summary>
    /// <param name="count">Quantidade máxima de sessões a retornar.</param>
    Task<IReadOnlyList<QuizSession>> GetRecentSessionsAsync(int count = 20);
    /// <summary>Retorna sessões dentro de um intervalo de datas.</summary>
    /// <param name="from">Data inicial (inclusive).</param>
    /// <param name="to">Data final (inclusive).</param>
    Task<IReadOnlyList<QuizSession>> GetSessionsByDateRangeAsync(DateTime from, DateTime to);
}
