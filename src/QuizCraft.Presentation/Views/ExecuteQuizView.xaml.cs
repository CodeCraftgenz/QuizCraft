using System.Windows.Controls;
using System.Windows.Input;
using QuizCraft.Domain.Entities;
using QuizCraft.Presentation.ViewModels;

namespace QuizCraft.Presentation.Views;

public partial class ExecuteQuizView : UserControl
{
    public ExecuteQuizView()
    {
        InitializeComponent();
    }

    private void Choice_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement fe && fe.Tag is Choice choice
            && DataContext is ExecuteQuizViewModel vm)
        {
            vm.SelectedChoice = choice;
        }
    }
}
