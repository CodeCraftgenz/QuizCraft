namespace QuizCraft.Domain.Entities;

/// <summary>
/// Tabela associativa (many-to-many) entre <see cref="Question"/> e <see cref="Tag"/>.
/// </summary>
public class QuestionTag
{
    /// <summary>Chave estrangeira para a questão.</summary>
    public int QuestionId { get; set; }
    /// <summary>Chave estrangeira para a tag.</summary>
    public int TagId { get; set; }

    // Navegações
    /// <summary>Questão associada.</summary>
    public Question Question { get; set; } = null!;
    /// <summary>Tag associada.</summary>
    public Tag Tag { get; set; } = null!;
}
