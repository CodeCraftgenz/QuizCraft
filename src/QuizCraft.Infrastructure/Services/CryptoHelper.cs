using System.Security.Cryptography;
using System.Text;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Helper de criptografia. Fornece funcoes de protecao local (DPAPI)
/// e hashing SHA-256 para uso no licenciamento.
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// Protege (criptografa) dados usando DPAPI vinculado ao usuario atual.
    /// Apenas o mesmo usuario no mesmo dispositivo pode descriptografar.
    /// </summary>
    /// <param name="data">Texto a ser protegido.</param>
    /// <returns>Dados criptografados em Base64.</returns>
    public static string Protect(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    /// <summary>
    /// Desprotege (descriptografa) dados previamente protegidos com DPAPI.
    /// </summary>
    /// <param name="protectedData">Dados criptografados em Base64.</param>
    /// <returns>Texto original descriptografado.</returns>
    public static string Unprotect(string protectedData)
    {
        var bytes = Convert.FromBase64String(protectedData);
        var unprotectedBytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(unprotectedBytes);
    }

    /// <summary>
    /// Gera o hash SHA-256 de um texto, retornando em formato hexadecimal.
    /// </summary>
    /// <param name="input">Texto de entrada.</param>
    /// <returns>Hash SHA-256 em hexadecimal (lowercase).</returns>
    public static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
