namespace QuizCraft.Domain.Enums;

/// <summary>
/// Tipos de questão suportados pelo sistema.
/// </summary>
public enum QuestionType
{
    /// <summary>Múltipla Escolha — apenas uma alternativa correta.</summary>
    MultipleChoice = 0,
    /// <summary>Verdadeiro ou Falso.</summary>
    TrueFalse = 1,
    /// <summary>Resposta Curta — o usuário digita a resposta.</summary>
    ShortAnswer = 2,
    /// <summary>Múltipla Seleção — várias alternativas podem estar corretas.</summary>
    MultipleSelection = 3
}
