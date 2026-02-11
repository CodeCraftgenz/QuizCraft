using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using QuizCraft.Application.Services;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Presentation.ViewModels;
using QuizCraft.Presentation.Views;
using Serilog;

namespace QuizCraft.Presentation;

/// <summary>
/// Janela principal do QuizCraft. Gerencia a navegação entre páginas via menu lateral.
/// </summary>
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly IServiceProvider _services;
    /// <summary>Flag para evitar navegação simultânea.</summary>
    private bool _isNavigating;

    /// <summary>Inicializa a janela com o provedor de serviços (DI).</summary>
    public MainWindow(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    /// <summary>Ao carregar a janela, seleciona o Dashboard como página inicial.</summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        NavListBox.SelectedIndex = 0;
    }

    /// <summary>Navega ao selecionar um item no menu lateral principal.</summary>
    private async void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavListBox.SelectedItem is ListBoxItem item && item.Tag is string tag)
        {
            // Limpa seleção do menu de rodapé para evitar conflito visual
            FooterNavListBox.SelectedIndex = -1;
            await NavigateToAsync(tag);
        }
    }

    /// <summary>Navega ao selecionar um item no menu de rodapé (Configurações, Ajuda).</summary>
    private async void FooterNavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FooterNavListBox.SelectedItem is ListBoxItem item && item.Tag is string tag)
        {
            // Limpa seleção do menu principal para evitar conflito visual
            NavListBox.SelectedIndex = -1;
            await NavigateToAsync(tag);
        }
    }

    /// <summary>
    /// Navega para uma página, criando o ViewModel e a View correspondentes.
    /// Usa guard flag para evitar navegação simultânea.
    /// </summary>
    private async Task NavigateToAsync(string page)
    {
        // Evita navegação concorrente
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            FrameworkElement? view = null;

            switch (page)
            {
                case "Dashboard":
                    var dashVm = _services.GetRequiredService<DashboardViewModel>();
                    await dashVm.InitializeAsync();
                    view = new DashboardView { DataContext = dashVm };
                    break;

                case "Subjects":
                    var subVm = _services.GetRequiredService<SubjectsViewModel>();
                    await subVm.InitializeAsync();
                    view = new SubjectsView { DataContext = subVm };
                    break;

                case "Questions":
                    var qVm = _services.GetRequiredService<QuestionsViewModel>();
                    await qVm.InitializeAsync();
                    view = new QuestionsView { DataContext = qVm };
                    break;

                case "CreateQuiz":
                    var quizService = _services.GetRequiredService<QuizService>();
                    var questionRepo = _services.GetRequiredService<IQuestionRepository>();
                    var context = _services.GetRequiredService<Infrastructure.Data.QuizCraftDbContext>();
                    var createVm = new CreateQuizViewModel(context, quizService, questionRepo, OnStartQuiz);
                    await createVm.InitializeAsync();
                    view = new CreateQuizView { DataContext = createVm };
                    break;

                case "Review":
                    var revVm = _services.GetRequiredService<ReviewViewModel>();
                    await revVm.InitializeAsync();
                    view = new ReviewView { DataContext = revVm };
                    break;

                case "History":
                    var sessionRepo = _services.GetRequiredService<IQuizSessionRepository>();
                    var histVm = new HistoryViewModel(sessionRepo, OnViewSession);
                    await histVm.InitializeAsync();
                    view = new HistoryView { DataContext = histVm };
                    break;

                case "Settings":
                    var settVm = _services.GetRequiredService<SettingsViewModel>();
                    await settVm.InitializeAsync();
                    view = new SettingsView { DataContext = settVm };
                    break;

                case "Help":
                    var helpVm = _services.GetRequiredService<HelpViewModel>();
                    await helpVm.InitializeAsync();
                    view = new HelpView { DataContext = helpVm };
                    break;
            }

            if (view != null)
                ContentArea.Content = view;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Navigation error for page {Page}", page);
            MessageBox.Show($"Erro ao navegar para {page}:\n{ex.Message}",
                "QuizCraft", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            _isNavigating = false;
        }
    }

    /// <summary>Callback chamado ao iniciar um quiz: cria a View de execução.</summary>
    private void OnStartQuiz(QuizSession session, IReadOnlyList<Question> questions)
    {
        var quizService = _services.GetRequiredService<QuizService>();
        var vm = new ExecuteQuizViewModel(quizService, OnFinishQuiz);
        vm.Initialize(session, questions);
        ContentArea.Content = new ExecuteQuizView { DataContext = vm };
    }

    /// <summary>Callback chamado ao finalizar um quiz: navega para a tela de resultados.</summary>
    private async void OnFinishQuiz(QuizSession session)
    {
        var sessionRepo = _services.GetRequiredService<IQuizSessionRepository>();
        var vm = new ResultsViewModel(sessionRepo);
        await vm.LoadSessionAsync(session.Id);
        ContentArea.Content = new ResultsView { DataContext = vm };
    }

    /// <summary>Callback chamado ao visualizar uma sessão do histórico.</summary>
    private async void OnViewSession(int sessionId)
    {
        var sessionRepo = _services.GetRequiredService<IQuizSessionRepository>();
        var vm = new ResultsViewModel(sessionRepo);
        await vm.LoadSessionAsync(sessionId);
        ContentArea.Content = new ResultsView { DataContext = vm };
    }
}
