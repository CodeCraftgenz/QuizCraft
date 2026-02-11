using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Repositorio de questoes com consultas especializadas (detalhes, busca, quiz, revisao).
/// Herda operacoes CRUD do repositorio generico.
/// </summary>
public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    /// <summary>Inicializa o repositorio de questoes.</summary>
    public QuestionRepository(QuizCraftDbContext context) : base(context) { }

    /// <summary>
    /// Busca uma questao pelo ID com todos os relacionamentos carregados
    /// (alternativas, tags, topico, disciplina e dominio).
    /// </summary>
    public async Task<Question?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(q => q.Choices.OrderBy(c => c.Order))
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Topic).ThenInclude(t => t.Subject)
            .Include(q => q.Mastery)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    /// <summary>
    /// Retorna todas as questoes de um topico, ordenadas pela data de criacao (mais recentes primeiro).
    /// </summary>
    public async Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId)
    {
        return await _dbSet
            .Include(q => q.Choices)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Where(q => q.TopicId == topicId)
            .OrderByDescending(q => q.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Pesquisa questoes com filtros combinados e paginacao.
    /// </summary>
    public async Task<IReadOnlyList<Question>> SearchAsync(string? searchText, int? topicId, int? subjectId,
        int? difficulty, List<int>? tagIds, int page = 1, int pageSize = 50)
    {
        var query = BuildSearchQuery(searchText, topicId, subjectId, difficulty, tagIds);

        return await query
            .Include(q => q.Choices)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Topic).ThenInclude(t => t.Subject)
            .OrderByDescending(q => q.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Retorna a contagem total de resultados para os mesmos filtros de pesquisa (para paginacao).
    /// </summary>
    public async Task<int> SearchCountAsync(string? searchText, int? topicId, int? subjectId,
        int? difficulty, List<int>? tagIds)
    {
        return await BuildSearchQuery(searchText, topicId, subjectId, difficulty, tagIds).CountAsync();
    }

    /// <summary>
    /// Retorna questoes para montar um quiz, com filtros por disciplina, topico, tags e dificuldade.
    /// Suporta ordenacao aleatoria para modo quiz.
    /// </summary>
    public async Task<IReadOnlyList<Question>> GetForQuizAsync(int? subjectId, int? topicId,
        List<int>? tagIds, int? minDifficulty, int? maxDifficulty, int count, bool randomize)
    {
        var query = _dbSet
            .Include(q => q.Choices)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Topic).ThenInclude(t => t.Subject)
            .Include(q => q.Mastery)
            .AsQueryable();

        // Aplicacao dinamica de filtros opcionais
        if (subjectId.HasValue)
            query = query.Where(q => q.Topic.SubjectId == subjectId.Value);
        if (topicId.HasValue)
            query = query.Where(q => q.TopicId == topicId.Value);
        if (tagIds is { Count: > 0 })
            query = query.Where(q => q.QuestionTags.Any(qt => tagIds.Contains(qt.TagId)));
        if (minDifficulty.HasValue)
            query = query.Where(q => q.Difficulty >= minDifficulty.Value);
        if (maxDifficulty.HasValue)
            query = query.Where(q => q.Difficulty <= maxDifficulty.Value);

        // Ordena aleatoriamente para quiz ou sequencialmente por ID
        if (randomize)
            query = query.OrderBy(_ => EF.Functions.Random());
        else
            query = query.OrderBy(q => q.Id);

        return await query.Take(count).ToListAsync();
    }

    /// <summary>
    /// Retorna questoes com revisao vencida (NextReviewAt <= agora),
    /// ordenadas pela data de revisao mais antiga.
    /// </summary>
    public async Task<IReadOnlyList<Question>> GetDueForReviewAsync(DateTime asOf, int? subjectId, int count)
    {
        var query = _dbSet
            .Include(q => q.Choices)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Topic).ThenInclude(t => t.Subject)
            .Include(q => q.Mastery)
            .Where(q => q.Mastery != null && q.Mastery.NextReviewAt <= asOf);

        if (subjectId.HasValue)
            query = query.Where(q => q.Topic.SubjectId == subjectId.Value);

        return await query
            .OrderBy(q => q.Mastery!.NextReviewAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Constroi a query de pesquisa com filtros opcionais (texto, topico, disciplina, dificuldade, tags).
    /// </summary>
    private IQueryable<Question> BuildSearchQuery(string? searchText, int? topicId,
        int? subjectId, int? difficulty, List<int>? tagIds)
    {
        var query = _dbSet.AsQueryable();

        // Busca textual no enunciado, explicacao e alternativas
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            query = query.Where(q =>
                q.Statement.ToLower().Contains(search) ||
                (q.Explanation != null && q.Explanation.ToLower().Contains(search)) ||
                q.Choices.Any(c => c.Text.ToLower().Contains(search)));
        }

        if (topicId.HasValue)
            query = query.Where(q => q.TopicId == topicId.Value);
        if (subjectId.HasValue)
            query = query.Where(q => q.Topic.SubjectId == subjectId.Value);
        if (difficulty.HasValue)
            query = query.Where(q => q.Difficulty == difficulty.Value);
        if (tagIds is { Count: > 0 })
            query = query.Where(q => q.QuestionTags.Any(qt => tagIds.Contains(qt.TagId)));

        return query;
    }
}
