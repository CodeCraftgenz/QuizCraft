namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa um conjunto de estudo salvo, contendo filtros pré-definidos
/// para gerar quizzes rapidamente.
/// </summary>
public class StudySet
{
    /// <summary>Identificador único do conjunto de estudo.</summary>
    public int Id { get; set; }
    /// <summary>Nome do conjunto de estudo.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Descrição opcional do conjunto.</summary>
    public string? Description { get; set; }
    /// <summary>Filtros salvos em JSON (matéria, tópicos, tags, dificuldade, etc.).</summary>
    public string? FiltersJson { get; set; }
    /// <summary>Data de criação do conjunto de estudo.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
