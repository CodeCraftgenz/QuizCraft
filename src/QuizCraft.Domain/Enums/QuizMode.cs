namespace QuizCraft.Domain.Enums;

/// <summary>
/// Modos de quiz disponíveis no aplicativo.
/// </summary>
public enum QuizMode
{
    /// <summary>Treino — feedback imediato após cada questão.</summary>
    Training = 0,
    /// <summary>Prova — resultado exibido somente ao final.</summary>
    Exam = 1,
    /// <summary>Revisão de Erros — repete apenas questões respondidas incorretamente.</summary>
    ErrorReview = 2,
    /// <summary>Revisão Espaçada — baseada no algoritmo de repetição espaçada.</summary>
    SpacedReview = 3
}
