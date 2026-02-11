using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizCraft.Application.Services;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de execução de quiz. Controla navegação entre questões, cronômetro, respostas e finalização.
/// </summary>
public partial class ExecuteQuizViewModel : BaseViewModel
{
    private readonly QuizService _quizService;
    /// <summary>Callback executado ao finalizar o quiz.</summary>
    private readonly Action<QuizSession> _onFinishQuiz;

    private QuizSession _session = null!;
    private IReadOnlyList<Question> _questions = [];
    /// <summary>Timer que incrementa o tempo a cada segundo.</summary>
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    /// <summary>Momento em que a questão atual foi carregada.</summary>
    private DateTime _questionStartTime;
    /// <summary>Lista de respostas registradas durante o quiz.</summary>
    private readonly List<AnswerRecord> _answers = [];

    /// <summary>Índice da questão atual (zero-based). Notifica CurrentDisplayIndex.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentDisplayIndex))]
    private int _currentIndex;
    /// <summary>Total de questões no quiz.</summary>
    [ObservableProperty] private int _totalQuestions;
    /// <summary>Questão sendo exibida atualmente.</summary>
    [ObservableProperty] private Question? _currentQuestion;
    /// <summary>Alternativa selecionada pelo usuário.</summary>
    [ObservableProperty] private Choice? _selectedChoice;
    /// <summary>Resposta digitada (para questões de resposta curta).</summary>
    [ObservableProperty] private string _shortAnswer = string.Empty;
    /// <summary>Indica se o usuário já respondeu a questão atual.</summary>
    [ObservableProperty] private bool _hasAnswered;
    /// <summary>Indica se a resposta está correta.</summary>
    [ObservableProperty] private bool _isCorrect;
    /// <summary>Indica se a questão está marcada para revisão.</summary>
    [ObservableProperty] private bool _markedForReview;
    /// <summary>Tempo total decorrido em segundos.</summary>
    [ObservableProperty] private int _elapsedSeconds;
    /// <summary>Quantidade de respostas corretas até o momento.</summary>
    [ObservableProperty] private int _correctCount;
    /// <summary>Indica se o quiz está pausado.</summary>
    [ObservableProperty] private bool _isPaused;
    /// <summary>Indica se a explicação está visível.</summary>
    [ObservableProperty] private bool _showExplanation;
    /// <summary>Modo do quiz (Treino, Prova, etc.).</summary>
    [ObservableProperty] private QuizMode _mode;
    /// <summary>Progresso percentual do quiz (0 a 100).</summary>
    [ObservableProperty] private double _progress;
    /// <summary>Tempo restante em segundos (null se sem timer).</summary>
    [ObservableProperty] private int? _timerRemaining;

    /// <summary>Índice exibido ao usuário (1-based).</summary>
    public int CurrentDisplayIndex => CurrentIndex + 1;

    /// <summary>Alternativas da questão atual para exibição.</summary>
    public ObservableCollection<Choice> CurrentChoices { get; } = new();

    /// <summary>Inicializa o ViewModel com o serviço de quiz e callback de finalização.</summary>
    public ExecuteQuizViewModel(QuizService quizService, Action<QuizSession> onFinishQuiz)
    {
        _quizService = quizService;
        _onFinishQuiz = onFinishQuiz;
        _timer.Tick += (_, _) =>
        {
            if (!IsPaused)
            {
                ElapsedSeconds++;
                // Verifica se há limite de tempo e se esgotou
                if (_session.TimeLimitSeconds.HasValue)
                {
                    TimerRemaining = _session.TimeLimitSeconds.Value - ElapsedSeconds;
                    if (TimerRemaining <= 0)
                        _ = FinishQuizAsync();
                }
            }
        };
    }

    /// <summary>Inicializa a sessão de quiz com as questões selecionadas.</summary>
    public void Initialize(QuizSession session, IReadOnlyList<Question> questions)
    {
        _session = session;
        _questions = questions;
        TotalQuestions = questions.Count;
        Mode = session.Mode;
        CurrentIndex = 0;
        CorrectCount = 0;
        ElapsedSeconds = 0;
        _answers.Clear();
        TimerRemaining = session.TimeLimitSeconds;

        LoadQuestion(0);
        _timer.Start();
    }

    /// <summary>Carrega uma questão pelo índice, restaurando resposta anterior se houver.</summary>
    private void LoadQuestion(int index)
    {
        if (index < 0 || index >= _questions.Count) return;

        CurrentIndex = index;
        CurrentQuestion = _questions[index];
        SelectedChoice = null;
        ShortAnswer = string.Empty;
        HasAnswered = false;
        IsCorrect = false;
        ShowExplanation = false;
        MarkedForReview = false;
        _questionStartTime = DateTime.UtcNow;
        Progress = (double)(index) / TotalQuestions * 100;

        CurrentChoices.Clear();
        foreach (var c in CurrentQuestion.Choices.OrderBy(c => c.Order))
            CurrentChoices.Add(c);

        // Restaura resposta anterior ao navegar de volta
        var existing = _answers.FirstOrDefault(a => a.QuestionId == CurrentQuestion.Id);
        if (existing != null)
        {
            HasAnswered = true;
            IsCorrect = existing.IsCorrect;
            MarkedForReview = existing.MarkedForReview;
            if (existing.SelectedChoiceId.HasValue)
                SelectedChoice = CurrentChoices.FirstOrDefault(c => c.Id == existing.SelectedChoiceId);
        }
    }

    /// <summary>Submete a resposta do usuário para a questão atual.</summary>
    [RelayCommand]
    private async Task SubmitAnswerAsync()
    {
        if (CurrentQuestion == null || HasAnswered) return;

        // Calcula o tempo gasto na questão
        var timeSpent = (int)(DateTime.UtcNow - _questionStartTime).TotalSeconds;
        bool correct;
        string? answerJson;

        if (CurrentQuestion.Type == QuestionType.ShortAnswer)
        {
            var correctAnswer = CurrentQuestion.Choices.FirstOrDefault(c => c.IsCorrect)?.Text ?? "";
            correct = string.Equals(ShortAnswer.Trim(), correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            answerJson = ShortAnswer;
        }
        else
        {
            if (SelectedChoice == null) return;
            correct = SelectedChoice.IsCorrect;
            answerJson = SelectedChoice.Id.ToString();
        }

        HasAnswered = true;
        IsCorrect = correct;
        if (correct) CorrectCount++;

        // Exibe explicação apenas no modo Treino
        if (Mode == QuizMode.Training)
            ShowExplanation = true;

        // Registra a resposta
        var record = new AnswerRecord
        {
            QuestionId = CurrentQuestion.Id,
            SelectedChoiceId = SelectedChoice?.Id,
            AnswerJson = answerJson,
            IsCorrect = correct,
            TimeSeconds = timeSpent,
            MarkedForReview = MarkedForReview,
            Order = CurrentIndex
        };

        // Substitui resposta anterior se houver (navegação)
        _answers.RemoveAll(a => a.QuestionId == CurrentQuestion.Id);
        _answers.Add(record);

        await _quizService.RecordAnswerAsync(
            _session.Id, CurrentQuestion.Id, answerJson,
            correct, timeSpent, MarkedForReview, CurrentIndex);
    }

    /// <summary>Avança para a próxima questão.</summary>
    [RelayCommand]
    private void NextQuestion()
    {
        // No modo Prova, submete automaticamente se há alternativa selecionada
        if (!HasAnswered && Mode == QuizMode.Exam && SelectedChoice != null)
            _ = SubmitAnswerAsync();

        if (CurrentIndex < TotalQuestions - 1)
            LoadQuestion(CurrentIndex + 1);
    }

    /// <summary>Volta para a questão anterior.</summary>
    [RelayCommand]
    private void PreviousQuestion()
    {
        if (CurrentIndex > 0)
            LoadQuestion(CurrentIndex - 1);
    }

    /// <summary>Alterna a marcação da questão para revisão posterior.</summary>
    [RelayCommand]
    private void ToggleMarkForReview()
    {
        MarkedForReview = !MarkedForReview;
        var existing = _answers.FirstOrDefault(a => a.QuestionId == CurrentQuestion?.Id);
        if (existing != null)
            existing.MarkedForReview = MarkedForReview;
    }

    /// <summary>Alterna entre pausado e em andamento.</summary>
    [RelayCommand]
    private void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    /// <summary>Finaliza o quiz, para o timer e navega para resultados.</summary>
    [RelayCommand]
    private async Task FinishQuizAsync()
    {
        _timer.Stop();
        var session = await _quizService.FinishSessionAsync(_session.Id);
        _onFinishQuiz(session);
    }

    /// <summary>Indica se o quiz pode ser finalizado (pelo menos uma resposta).</summary>
    public bool CanFinish => _answers.Count == TotalQuestions || _answers.Count > 0;

    /// <summary>Registro interno de resposta do usuário durante o quiz.</summary>
    private class AnswerRecord
    {
        public int QuestionId { get; set; }
        public int? SelectedChoiceId { get; set; }
        public string? AnswerJson { get; set; }
        public bool IsCorrect { get; set; }
        public int TimeSeconds { get; set; }
        public bool MarkedForReview { get; set; }
        public int Order { get; set; }
    }
}
