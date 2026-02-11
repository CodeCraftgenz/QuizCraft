namespace QuizCraft.Domain.Models;

/// <summary>
/// Estado atual da licenca do usuario.
/// </summary>
public enum LicenseState
{
    /// <summary>Nenhuma licenca encontrada localmente.</summary>
    NotFound,
    /// <summary>Licenca valida e ativa.</summary>
    Valid,
    /// <summary>Licenca invalida ou expirada.</summary>
    Invalid,
    /// <summary>Erro ao validar (sem conexao, por exemplo).</summary>
    Error
}

/// <summary>
/// Registro local da licenca armazenada no dispositivo.
/// </summary>
public class LicenseRecord
{
    /// <summary>Email do usuario que ativou a licenca.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Chave de licenca retornada pelo servidor.</summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>Identificador unico do hardware (fingerprint).</summary>
    public string HardwareId { get; set; } = string.Empty;

    /// <summary>Data/hora da ativacao.</summary>
    public DateTime ActivatedAt { get; set; }

    /// <summary>Data/hora da ultima validacao com o servidor.</summary>
    public DateTime LastValidatedAt { get; set; }
}

/// <summary>
/// Resultado da validacao de licenca retornado pela API.
/// </summary>
public class LicenseValidationResult
{
    /// <summary>Indica se a licenca e valida.</summary>
    public bool Valid { get; set; }

    /// <summary>Mensagem descritiva do resultado.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Chave de licenca (quando valida).</summary>
    public string? LicenseKey { get; set; }

    /// <summary>Data de ativacao (quando valida).</summary>
    public DateTime? ActivatedAt { get; set; }
}

/// <summary>
/// Resultado da ativacao de dispositivo retornado pela API.
/// </summary>
public class DeviceActivationResult
{
    /// <summary>Indica se a ativacao foi bem-sucedida.</summary>
    public bool Success { get; set; }

    /// <summary>Mensagem descritiva do resultado.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Chave de licenca gerada/existente.</summary>
    public string? LicenseKey { get; set; }

    /// <summary>Nome do aplicativo.</summary>
    public string? AppName { get; set; }
}
