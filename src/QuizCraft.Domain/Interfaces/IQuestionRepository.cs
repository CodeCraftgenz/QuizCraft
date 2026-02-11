using QuizCraft.Domain.Entities;

namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Repositório especializado para questões, com métodos de busca e filtragem avançados.
/// </summary>
public interface IQuestionRepository : IRepository<Question>
{
    /// <summary>Busca questões com filtros combinados e paginação.</summary>
    /// <param name="searchText">Texto a buscar no enunciado.</param>
    /// <param name="topicId">Filtro por tópico.</param>
    /// <param name="subjectId">Filtro por matéria.</param>
    /// <param name="difficulty">Filtro por dificuldade.</param>
    /// <param name="tagIds">Filtro por tags.</param>
    /// <param name="page">Página atual (base 1).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    Task<IReadOnlyList<Question>> SearchAsync(string? searchText, int? topicId, int? subjectId,
        int? difficulty, List<int>? tagIds, int page = 1, int pageSize = 50);
    /// <summary>Conta o total de questões que atendem aos filtros (para paginação).</summary>
    Task<int> SearchCountAsync(string? searchText, int? topicId, int? subjectId,
        int? difficulty, List<int>? tagIds);
    /// <summary>Obtém uma questão com todas as suas relações carregadas (choices, tags, mastery).</summary>
    Task<Question?> GetWithDetailsAsync(int id);
    /// <summary>Retorna todas as questões de um tópico específico.</summary>
    Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId);
    /// <summary>Seleciona questões para montar um quiz com base nos filtros e quantidade desejada.</summary>
    /// <param name="randomize">Se verdadeiro, embaralha a seleção.</param>
    Task<IReadOnlyList<Question>> GetForQuizAsync(int? subjectId, int? topicId,
        List<int>? tagIds, int? minDifficulty, int? maxDifficulty, int count, bool randomize);
    /// <summary>Retorna questões pendentes de revisão espaçada até a data informada.</summary>
    /// <param name="asOf">Data de referência para verificar questões vencidas.</param>
    Task<IReadOnlyList<Question>> GetDueForReviewAsync(DateTime asOf, int? subjectId, int count);
}
