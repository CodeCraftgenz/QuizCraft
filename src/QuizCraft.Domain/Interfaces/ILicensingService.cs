using QuizCraft.Domain.Models;

namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Interface principal do servico de licenciamento.
/// Gerencia ativacao, validacao e verificacao periodica da licenca.
/// </summary>
public interface ILicensingService
{
    /// <summary>
    /// Verifica se existe uma licenca valida localmente.
    /// Tenta validar com o servidor se possivel.
    /// </summary>
    /// <returns>Estado atual da licenca.</returns>
    Task<LicenseState> CheckLicenseAsync();

    /// <summary>
    /// Ativa o dispositivo com o email informado.
    /// Registra a licenca localmente apos ativacao bem-sucedida.
    /// </summary>
    /// <param name="email">Email cadastrado na compra.</param>
    /// <returns>Resultado da ativacao.</returns>
    Task<DeviceActivationResult> ActivateAsync(string email);

    /// <summary>
    /// Remove a licenca local (logout).
    /// </summary>
    void RemoveLicense();

    /// <summary>
    /// Retorna o registro da licenca armazenada localmente, se existir.
    /// </summary>
    LicenseRecord? GetStoredLicense();
}
