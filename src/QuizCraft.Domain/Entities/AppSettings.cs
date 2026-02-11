namespace QuizCraft.Domain.Entities;

/// <summary>
/// Armazena configurações do aplicativo em pares chave-valor.
/// </summary>
public class AppSettings
{
    /// <summary>Identificador único da configuração.</summary>
    public int Id { get; set; }
    /// <summary>Chave da configuração (ex.: "theme", "defaultQuizSize").</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Valor da configuração.</summary>
    public string Value { get; set; } = string.Empty;
}
