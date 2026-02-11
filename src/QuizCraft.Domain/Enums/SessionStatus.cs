namespace QuizCraft.Domain.Enums;

/// <summary>
/// Status possíveis de uma sessão de quiz.
/// </summary>
public enum SessionStatus
{
    /// <summary>Em andamento — o usuário ainda está respondendo.</summary>
    InProgress = 0,
    /// <summary>Concluída — todas as questões foram respondidas.</summary>
    Completed = 1,
    /// <summary>Abandonada — o usuário saiu antes de finalizar.</summary>
    Abandoned = 2,
    /// <summary>Pausada — sessão interrompida temporariamente.</summary>
    Paused = 3
}
