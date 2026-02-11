using CommunityToolkit.Mvvm.ComponentModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel base abstrata para todas as telas. Fornece controle de loading e tratamento centralizado de erros.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    /// <summary>Indica se uma operação assíncrona está em andamento.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Mensagem de erro exibida ao usuário quando uma operação falha.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Limpa a mensagem de erro atual.</summary>
    protected void ClearError() => ErrorMessage = null;

    /// <summary>
    /// Executa uma ação assíncrona com controle automático de loading e tratamento de exceções.
    /// </summary>
    /// <param name="action">Ação assíncrona a ser executada.</param>
    protected async Task ExecuteWithLoadingAsync(Func<Task> action)
    {
        try
        {
            // Ativa o indicador de loading e limpa erros anteriores
            IsLoading = true;
            ClearError();
            await action();
        }
        catch (Exception ex)
        {
            // Captura a exceção e exibe a mensagem ao usuário
            ErrorMessage = ex.Message;
            Serilog.Log.Error(ex, "Error in {ViewModel}", GetType().Name);
        }
        finally
        {
            // Sempre desativa o loading, independente do resultado
            IsLoading = false;
        }
    }

    /// <summary>
    /// Inicialização assíncrona do ViewModel. Sobrescreva nas classes filhas para carregar dados.
    /// </summary>
    public virtual Task InitializeAsync() => Task.CompletedTask;
}
