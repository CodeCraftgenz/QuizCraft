namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa uma matéria/disciplina de estudo (ex.: Matemática, História).
/// </summary>
public class Subject
{
    /// <summary>Identificador único da matéria.</summary>
    public int Id { get; set; }
    /// <summary>Nome da matéria.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Descrição opcional da matéria.</summary>
    public string? Description { get; set; }
    /// <summary>Cor associada para exibição na interface (hex ou nome).</summary>
    public string? Color { get; set; }
    /// <summary>Data de criação do registro.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Data da última atualização.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegação: tópicos pertencentes a esta matéria
    /// <summary>Coleção de tópicos vinculados a esta matéria.</summary>
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
