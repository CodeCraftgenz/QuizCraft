using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Application.Services;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de criação de quiz. Permite configurar matéria, tópico, modo, dificuldade e iniciar uma sessão.
/// </summary>
public partial class CreateQuizViewModel : BaseViewModel
{
    private readonly QuizCraftDbContext _context;
    private readonly QuizService _quizService;
    private readonly IQuestionRepository _questionRepository;
    /// <summary>Callback executado ao iniciar o quiz, recebendo sessão e questões.</summary>
    private readonly Action<QuizSession, IReadOnlyList<Question>> _onStartQuiz;

    /// <summary>Matéria selecionada para filtrar questões.</summary>
    [ObservableProperty] private Subject? _selectedSubject;
    /// <summary>Tópico selecionado para filtrar questões.</summary>
    [ObservableProperty] private Topic? _selectedTopic;
    /// <summary>Modo do quiz (Treino, Prova, Revisão, etc.).</summary>
    [ObservableProperty] private QuizMode _selectedMode = QuizMode.Training;
    /// <summary>Quantidade desejada de questões no quiz.</summary>
    [ObservableProperty] private int _questionCount = 10;
    /// <summary>Dificuldade mínima (opcional).</summary>
    [ObservableProperty] private int? _minDifficulty;
    /// <summary>Dificuldade máxima (opcional).</summary>
    [ObservableProperty] private int? _maxDifficulty;
    /// <summary>Embaralhar ordem das questões.</summary>
    [ObservableProperty] private bool _randomize = true;
    /// <summary>Embaralhar alternativas de cada questão.</summary>
    [ObservableProperty] private bool _shuffleChoices = true;
    /// <summary>Ativar cronômetro por questão.</summary>
    [ObservableProperty] private bool _useTimer;
    /// <summary>Tempo limite em segundos por questão.</summary>
    [ObservableProperty] private int _timerSeconds = 60;
    /// <summary>Quantidade de questões disponíveis com os filtros atuais.</summary>
    [ObservableProperty] private int _availableQuestionCount;

    /// <summary>Lista de matérias disponíveis para seleção.</summary>
    public ObservableCollection<Subject> Subjects { get; } = new();
    /// <summary>Lista de tópicos da matéria selecionada.</summary>
    public ObservableCollection<Topic> Topics { get; } = new();

    /// <summary>Inicializa o ViewModel com as dependências e callback de início de quiz.</summary>
    public CreateQuizViewModel(QuizCraftDbContext context, QuizService quizService,
        IQuestionRepository questionRepository, Action<QuizSession, IReadOnlyList<Question>> onStartQuiz)
    {
        _context = context;
        _quizService = quizService;
        _questionRepository = questionRepository;
        _onStartQuiz = onStartQuiz;
    }

    /// <summary>Carrega matérias e contagem de questões disponíveis ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        var subjects = await _context.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        Subjects.Clear();
        foreach (var s in subjects) Subjects.Add(s);

        await UpdateAvailableCountAsync();
    }

    /// <summary>Ao trocar a matéria, recarrega tópicos e atualiza contagem.</summary>
    partial void OnSelectedSubjectChanged(Subject? value)
    {
        _ = LoadTopicsAsync();
        _ = UpdateAvailableCountAsync();
    }

    /// <summary>Ao trocar o tópico, atualiza a contagem de questões disponíveis.</summary>
    partial void OnSelectedTopicChanged(Topic? value)
    {
        _ = UpdateAvailableCountAsync();
    }

    /// <summary>Carrega os tópicos da matéria selecionada.</summary>
    private async Task LoadTopicsAsync()
    {
        Topics.Clear();
        if (SelectedSubject != null)
        {
            var topics = await _context.Topics
                .Where(t => t.SubjectId == SelectedSubject.Id)
                .OrderBy(t => t.Name).AsNoTracking().ToListAsync();
            foreach (var t in topics) Topics.Add(t);
        }
    }

    /// <summary>Atualiza a contagem de questões disponíveis com os filtros atuais.</summary>
    private async Task UpdateAvailableCountAsync()
    {
        AvailableQuestionCount = await _questionRepository.SearchCountAsync(
            null, SelectedTopic?.Id, SelectedSubject?.Id, null, null);
    }

    /// <summary>Monta as questões do quiz e inicia a sessão.</summary>
    [RelayCommand]
    private async Task StartQuizAsync()
    {
        // Não inicia se não há questões disponíveis
        if (AvailableQuestionCount == 0) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            // Limita a quantidade ao total disponível
            var count = Math.Min(QuestionCount, AvailableQuestionCount);
            var questions = await _quizService.BuildQuizQuestionsAsync(
                SelectedMode, SelectedSubject?.Id, SelectedTopic?.Id,
                null, MinDifficulty, MaxDifficulty,
                count, Randomize, ShuffleChoices);

            if (questions.Count == 0)
            {
                ErrorMessage = "Nenhuma questão encontrada com os filtros selecionados.";
                return;
            }

            var session = await _quizService.StartSessionAsync(
                SelectedMode, questions.Count,
                UseTimer ? TimerSeconds : null, null);

            _onStartQuiz(session, questions);
        });
    }
}
