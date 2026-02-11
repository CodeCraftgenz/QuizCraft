namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa o nível de domínio/maestria de uma questão para repetição espaçada.
/// Quanto maior o nível, maior o intervalo até a próxima revisão.
/// </summary>
public class Mastery
{
    /// <summary>Identificador único do registro de domínio.</summary>
    public int Id { get; set; }
    /// <summary>Chave estrangeira para a questão associada.</summary>
    public int QuestionId { get; set; }
    /// <summary>Nível atual de domínio (0 = novo, valores maiores = mais dominado).</summary>
    public int Level { get; set; }
    /// <summary>Data/hora da próxima revisão agendada.</summary>
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    /// <summary>Data/hora da última tentativa de resposta.</summary>
    public DateTime? LastAttemptAt { get; set; }
    /// <summary>Sequência atual de erros consecutivos.</summary>
    public int WrongStreak { get; set; }
    /// <summary>Sequência atual de acertos consecutivos.</summary>
    public int RightStreak { get; set; }
    /// <summary>Total de tentativas realizadas.</summary>
    public int TotalAttempts { get; set; }
    /// <summary>Total de respostas corretas.</summary>
    public int TotalCorrect { get; set; }

    /// <summary>Questão associada a este registro de domínio.</summary>
    public Question Question { get; set; } = null!;
}
