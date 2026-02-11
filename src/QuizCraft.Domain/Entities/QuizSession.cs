using QuizCraft.Domain.Enums;

namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa uma sessão de quiz (prova/treino) realizada pelo usuário.
/// </summary>
public class QuizSession
{
    /// <summary>Identificador único da sessão.</summary>
    public int Id { get; set; }
    /// <summary>Modo do quiz (treino, prova, revisão, etc.).</summary>
    public QuizMode Mode { get; set; }
    /// <summary>Status atual da sessão.</summary>
    public SessionStatus Status { get; set; } = SessionStatus.InProgress;
    /// <summary>Data/hora de início da sessão.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Data/hora de término (nulo se ainda em andamento).</summary>
    public DateTime? EndedAt { get; set; }
    /// <summary>Total de questões nesta sessão.</summary>
    public int TotalQuestions { get; set; }
    /// <summary>Quantidade de respostas corretas.</summary>
    public int CorrectCount { get; set; }
    /// <summary>Duração total da sessão em segundos.</summary>
    public int DurationSeconds { get; set; }
    /// <summary>Limite de tempo em segundos (nulo = sem limite).</summary>
    public int? TimeLimitSeconds { get; set; }
    /// <summary>Filtros aplicados na geração do quiz, armazenados em JSON.</summary>
    public string? FiltersJson { get; set; }

    /// <summary>Itens (respostas) desta sessão.</summary>
    public ICollection<QuizSessionItem> Items { get; set; } = new List<QuizSessionItem>();
}
