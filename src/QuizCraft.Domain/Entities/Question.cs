using QuizCraft.Domain.Enums;

namespace QuizCraft.Domain.Entities;

/// <summary>
/// Representa uma questão com enunciado, alternativas, tags e nível de domínio.
/// </summary>
public class Question
{
    /// <summary>Identificador único da questão.</summary>
    public int Id { get; set; }
    /// <summary>Chave estrangeira para o tópico ao qual a questão pertence.</summary>
    public int TopicId { get; set; }
    /// <summary>Tipo da questão (múltipla escolha, V/F, etc.).</summary>
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    /// <summary>Enunciado/texto da questão.</summary>
    public string Statement { get; set; } = string.Empty;
    /// <summary>Explicação exibida após a resposta (feedback).</summary>
    public string? Explanation { get; set; }
    /// <summary>Nível de dificuldade (1 = fácil, valores maiores = mais difícil).</summary>
    public int Difficulty { get; set; } = 1;
    /// <summary>Fonte ou referência bibliográfica da questão.</summary>
    public string? Source { get; set; }
    /// <summary>Caminho da imagem associada à questão, se houver.</summary>
    public string? ImagePath { get; set; }
    /// <summary>Data de criação do registro.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Data da última atualização.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegações
    /// <summary>Tópico ao qual esta questão pertence.</summary>
    public Topic Topic { get; set; } = null!;
    /// <summary>Alternativas (choices) desta questão.</summary>
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
    /// <summary>Tags associadas a esta questão (tabela associativa).</summary>
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    /// <summary>Dados de domínio/maestria para repetição espaçada.</summary>
    public Mastery? Mastery { get; set; }
}
