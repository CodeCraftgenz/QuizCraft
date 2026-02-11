namespace QuizCraft.Domain.Entities;

/// <summary>
/// Registra a atividade diária de estudo para controle de sequência (streak).
/// Cada registro representa um dia em que o usuário estudou.
/// </summary>
public class StudyStreak
{
    /// <summary>Identificador único do registro diário.</summary>
    public int Id { get; set; }
    /// <summary>Data do dia de estudo.</summary>
    public DateTime Date { get; set; }
    /// <summary>Quantidade de questões respondidas no dia.</summary>
    public int QuestionsAnswered { get; set; }
    /// <summary>Quantidade de respostas corretas no dia.</summary>
    public int CorrectAnswers { get; set; }
    /// <summary>Tempo total de estudo no dia, em segundos.</summary>
    public int StudyTimeSeconds { get; set; }
}
