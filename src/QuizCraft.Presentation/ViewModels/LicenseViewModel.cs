using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Domain.Models;
using Serilog;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel da tela de ativacao de licenca.
/// Gerencia o formulario de email e a comunicacao com o servico de licenciamento.
/// </summary>
public partial class LicenseViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<LicenseViewModel>();
    private readonly ILicensingService _licensingService;

    /// <summary>Email informado pelo usuario para ativacao.</summary>
    [ObservableProperty]
    private string _email = string.Empty;

    /// <summary>Mensagem de erro exibida na tela.</summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>Mensagem de sucesso exibida na tela.</summary>
    [ObservableProperty]
    private string _successMessage = string.Empty;

    /// <summary>Indica se uma operacao esta em andamento.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Indica se a ativacao foi concluida com sucesso.</summary>
    public bool IsActivated { get; private set; }

    /// <summary>
    /// Evento disparado quando a ativacao e concluida com sucesso.
    /// A janela deve se fechar ao receber este evento.
    /// </summary>
    public event Action? ActivationCompleted;

    /// <summary>
    /// Inicializa o ViewModel com o servico de licenciamento.
    /// </summary>
    public LicenseViewModel(ILicensingService licensingService)
    {
        _licensingService = licensingService;
    }

    /// <summary>
    /// Comando para ativar a licenca com o email informado.
    /// Valida o email, envia ao servidor e processa a resposta.
    /// </summary>
    [RelayCommand]
    private async Task ActivateAsync()
    {
        // Limpar mensagens anteriores
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        // Validar email
        var email = Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "Por favor, informe seu email.";
            return;
        }

        if (!IsValidEmail(email))
        {
            ErrorMessage = "Email invalido. Verifique e tente novamente.";
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _licensingService.ActivateAsync(email);

            if (result.Success)
            {
                IsActivated = true;
                SuccessMessage = "Licenca ativada com sucesso! Abrindo o app...";
                Logger.Information("Licenca ativada com sucesso para {Email}", email);

                // Aguardar um momento para o usuario ver a mensagem
                await Task.Delay(1500);
                ActivationCompleted?.Invoke();
            }
            else
            {
                ErrorMessage = result.Message;
                Logger.Warning("Falha na ativacao: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            Logger.Error(ex, "Erro ao ativar licenca");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Validacao simples de formato de email.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.ToLowerInvariant() || addr.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
