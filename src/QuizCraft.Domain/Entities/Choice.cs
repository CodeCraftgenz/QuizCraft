namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa uma alternativa (opção de resposta) de uma questão.
/// </summary>
public class Choice
{
    /// <summary>Identificador único da alternativa.</summary>
    public int Id { get; set; }
    /// <summary>Chave estrangeira para a questão à qual pertence.</summary>
    public int QuestionId { get; set; }
    /// <summary>Texto da alternativa.</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>Indica se esta alternativa é a correta.</summary>
    public bool IsCorrect { get; set; }
    /// <summary>Ordem de exibição da alternativa.</summary>
    public int Order { get; set; }

    /// <summary>Questão à qual esta alternativa pertence.</summary>
    public Question Question { get; set; } = null!;
}
