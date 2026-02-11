using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Repositorio de sessoes de quiz. Fornece consultas especializadas
/// com carregamento de itens, questoes e relacionamentos.
/// </summary>
public class QuizSessionRepository : Repository<QuizSession>, IQuizSessionRepository
{
    /// <summary>Inicializa o repositorio de sessoes.</summary>
    public QuizSessionRepository(QuizCraftDbContext context) : base(context) { }

    /// <summary>
    /// Busca uma sessao pelo ID com todos os itens e seus relacionamentos
    /// (questoes, alternativas, topicos, disciplinas e tags).
    /// </summary>
    public async Task<QuizSession?> GetWithItemsAsync(int id)
    {
        // Carrega a arvore completa de relacionamentos para exibicao detalhada
        return await _dbSet
            .Include(s => s.Items.OrderBy(i => i.Order))
                .ThenInclude(i => i.Question)
                    .ThenInclude(q => q.Choices)
            .Include(s => s.Items)
                .ThenInclude(i => i.Question)
                    .ThenInclude(q => q.Topic)
                        .ThenInclude(t => t.Subject)
            .Include(s => s.Items)
                .ThenInclude(i => i.Question)
                    .ThenInclude(q => q.QuestionTags)
                        .ThenInclude(qt => qt.Tag)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Retorna as sessoes concluidas mais recentes, ordenadas pela data de termino.
    /// </summary>
    /// <param name="count">Quantidade maxima de sessoes a retornar.</param>
    public async Task<IReadOnlyList<QuizSession>> GetRecentSessionsAsync(int count = 20)
    {
        return await _dbSet
            .Where(s => s.Status == SessionStatus.Completed)
            .OrderByDescending(s => s.EndedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Retorna sessoes concluidas em um intervalo de datas, com itens e topicos carregados.
    /// </summary>
    public async Task<IReadOnlyList<QuizSession>> GetSessionsByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(s => s.StartedAt >= from && s.StartedAt <= to && s.Status == SessionStatus.Completed)
            .Include(s => s.Items)
                .ThenInclude(i => i.Question)
                    .ThenInclude(q => q.Topic)
            .OrderByDescending(s => s.StartedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
