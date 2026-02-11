namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa um item (questão respondida) dentro de uma sessão de quiz.
/// </summary>
public class QuizSessionItem
{
    /// <summary>Identificador único do item.</summary>
    public int Id { get; set; }
    /// <summary>Chave estrangeira para a sessão.</summary>
    public int SessionId { get; set; }
    /// <summary>Chave estrangeira para a questão respondida.</summary>
    public int QuestionId { get; set; }
    /// <summary>Resposta selecionada pelo usuário, armazenada em JSON.</summary>
    public string? SelectedAnswerJson { get; set; }
    /// <summary>Indica se a resposta está correta.</summary>
    public bool IsCorrect { get; set; }
    /// <summary>Tempo gasto na questão em segundos.</summary>
    public int TimeSeconds { get; set; }
    /// <summary>Indica se o usuário marcou a questão para revisão posterior.</summary>
    public bool MarkedForReview { get; set; }
    /// <summary>Ordem de apresentação da questão na sessão.</summary>
    public int Order { get; set; }

    // Navegações
    /// <summary>Sessão à qual este item pertence.</summary>
    public QuizSession Session { get; set; } = null!;
    /// <summary>Questão respondida neste item.</summary>
    public Question Question { get; set; } = null!;
}
