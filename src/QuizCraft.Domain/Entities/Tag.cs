namespace QuizCraft.Domain.Entities;

/// <summary>
/// Etiqueta (tag) para categorizar e filtrar questões.
/// </summary>
public class Tag
{
    /// <summary>Identificador único da tag.</summary>
    public int Id { get; set; }
    /// <summary>Nome da tag (ex.: "Vestibular", "ENEM", "Concurso").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Questões associadas a esta tag (tabela associativa).</summary>
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
}
