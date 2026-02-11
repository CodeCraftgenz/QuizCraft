using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using QuizCraft.Presentation.ViewModels;

namespace QuizCraft.Presentation;

/// <summary>
/// Janela de ativacao de licenca. Exibida antes da janela principal
/// quando nao ha licenca valida no dispositivo.
/// </summary>
public partial class LicenseWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly LicenseViewModel _viewModel;

    /// <summary>
    /// Indica se a ativacao foi concluida com sucesso.
    /// </summary>
    public bool IsActivated => _viewModel.IsActivated;

    /// <summary>
    /// Inicializa a janela com o ViewModel de licenciamento.
    /// </summary>
    public LicenseWindow(LicenseViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();

        // Fechar janela quando a ativacao for concluida
        _viewModel.ActivationCompleted += () =>
        {
            Dispatcher.Invoke(() =>
            {
                DialogResult = true;
                Close();
            });
        };

        // Focar no campo de email ao abrir
        Loaded += (_, _) => EmailTextBox.Focus();
    }

    /// <summary>
    /// Abre links externos no navegador padrao.
    /// </summary>
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
