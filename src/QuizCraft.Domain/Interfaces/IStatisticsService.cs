namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Estatísticas consolidadas para o dashboard principal.
/// </summary>
/// <param name="TotalQuestions">Total de questões cadastradas.</param>
/// <param name="QuestionsStudied">Questões já estudadas pelo menos uma vez.</param>
/// <param name="AccuracyRate7Days">Taxa de acerto dos últimos 7 dias.</param>
/// <param name="AccuracyRate30Days">Taxa de acerto dos últimos 30 dias.</param>
/// <param name="AverageTimeSeconds">Tempo médio por questão em segundos.</param>
/// <param name="CurrentStreak">Sequência atual de dias consecutivos de estudo.</param>
/// <param name="TotalSessions">Total de sessões realizadas.</param>
/// <param name="DueForReview">Questões pendentes de revisão espaçada.</param>
public record DashboardStats(
    int TotalQuestions,
    int QuestionsStudied,
    double AccuracyRate7Days,
    double AccuracyRate30Days,
    double AverageTimeSeconds,
    int CurrentStreak,
    int TotalSessions,
    int DueForReview
);

/// <summary>Desempenho do usuário em um tópico específico.</summary>
public record TopicPerformance(string TopicName, string SubjectName, double AccuracyRate, int TotalAttempts);
/// <summary>Desempenho diário do usuário (para gráficos de evolução).</summary>
public record DailyPerformance(DateTime Date, double AccuracyRate, int QuestionsAnswered);

/// <summary>
/// Serviço responsável por calcular e fornecer estatísticas de desempenho.
/// </summary>
public interface IStatisticsService
{
    /// <summary>Retorna as estatísticas consolidadas para o dashboard.</summary>
    Task<DashboardStats> GetDashboardStatsAsync();
    /// <summary>Retorna os tópicos com pior desempenho (pontos fracos).</summary>
    /// <param name="count">Quantidade de tópicos a retornar.</param>
    Task<IReadOnlyList<TopicPerformance>> GetWeakestTopicsAsync(int count = 5);
    /// <summary>Retorna o desempenho diário dos últimos N dias.</summary>
    /// <param name="days">Quantidade de dias a considerar.</param>
    Task<IReadOnlyList<DailyPerformance>> GetDailyPerformanceAsync(int days = 30);
    /// <summary>Retorna o desempenho agrupado por tópico, opcionalmente filtrado por matéria.</summary>
    Task<IReadOnlyList<TopicPerformance>> GetPerformanceByTopicAsync(int? subjectId = null);
    /// <summary>Registra a atividade de estudo do dia atual.</summary>
    Task RecordStudyDayAsync(int questionsAnswered, int correctAnswers, int timeSeconds);
}
