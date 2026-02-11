namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa um tópico dentro de uma matéria. Suporta hierarquia (sub-tópicos).
/// </summary>
public class Topic
{
    /// <summary>Identificador único do tópico.</summary>
    public int Id { get; set; }
    /// <summary>Chave estrangeira para a matéria à qual o tópico pertence.</summary>
    public int SubjectId { get; set; }
    /// <summary>Chave estrangeira para o tópico pai (nulo se for raiz).</summary>
    public int? ParentTopicId { get; set; }
    /// <summary>Nome do tópico.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Descrição opcional do tópico.</summary>
    public string? Description { get; set; }
    /// <summary>Data de criação do registro.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegações
    /// <summary>Matéria à qual este tópico pertence.</summary>
    public Subject Subject { get; set; } = null!;
    /// <summary>Tópico pai (nulo se for tópico raiz).</summary>
    public Topic? ParentTopic { get; set; }
    /// <summary>Sub-tópicos filhos deste tópico.</summary>
    public ICollection<Topic> SubTopics { get; set; } = new List<Topic>();
    /// <summary>Questões vinculadas a este tópico.</summary>
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
