using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de gerenciamento de questões. Permite buscar, criar, editar e excluir questões com paginação.
/// </summary>
public partial class QuestionsViewModel : BaseViewModel
{
    private readonly QuizCraftDbContext _context;
    private readonly IQuestionRepository _questionRepository;

    // Filtros de busca
    /// <summary>Texto de busca para filtrar questões.</summary>
    [ObservableProperty] private string _searchText = string.Empty;
    /// <summary>Matéria selecionada como filtro.</summary>
    [ObservableProperty] private Subject? _filterSubject;
    /// <summary>Tópico selecionado como filtro.</summary>
    [ObservableProperty] private Topic? _filterTopic;
    /// <summary>Nível de dificuldade como filtro.</summary>
    [ObservableProperty] private int? _filterDifficulty;
    /// <summary>Página atual da paginação.</summary>
    [ObservableProperty] private int _currentPage = 1;
    /// <summary>Total de páginas disponíveis.</summary>
    [ObservableProperty] private int _totalPages = 1;
    /// <summary>Total de questões encontradas na busca.</summary>
    [ObservableProperty] private int _totalCount;

    // Campos do editor de questão
    /// <summary>Indica se o painel de edição está aberto.</summary>
    [ObservableProperty] private bool _isEditorOpen;
    /// <summary>Indica se é questão nova (true) ou edição (false).</summary>
    [ObservableProperty] private bool _isNewQuestion = true;
    /// <summary>ID da questão sendo editada.</summary>
    [ObservableProperty] private int _editingQuestionId;
    /// <summary>Enunciado da questão no editor.</summary>
    [ObservableProperty] private string _editorStatement = string.Empty;
    /// <summary>Explicação da resposta correta.</summary>
    [ObservableProperty] private string? _editorExplanation;
    /// <summary>Tipo da questão (múltipla escolha, resposta curta, etc.).</summary>
    [ObservableProperty] private QuestionType _editorType = QuestionType.MultipleChoice;
    /// <summary>Nível de dificuldade (1 a 5).</summary>
    [ObservableProperty] private int _editorDifficulty = 1;
    /// <summary>Fonte/referência da questão.</summary>
    [ObservableProperty] private string? _editorSource;
    /// <summary>Tópico associado à questão no editor.</summary>
    [ObservableProperty] private Topic? _editorTopic;
    /// <summary>Tags da questão separadas por vírgula.</summary>
    [ObservableProperty] private string _editorTagsText = string.Empty;

    /// <summary>Lista de questões exibidas na página atual.</summary>
    public ObservableCollection<Question> Questions { get; } = new();
    /// <summary>Lista de matérias para o filtro.</summary>
    public ObservableCollection<Subject> Subjects { get; } = new();
    /// <summary>Lista de tópicos para o filtro.</summary>
    public ObservableCollection<Topic> Topics { get; } = new();
    /// <summary>Todos os tópicos disponíveis (para o editor).</summary>
    public ObservableCollection<Topic> AllTopics { get; } = new();
    /// <summary>Alternativas editáveis da questão no editor.</summary>
    public ObservableCollection<EditableChoice> EditorChoices { get; } = new();

    /// <summary>Inicializa o ViewModel com o contexto e repositório de questões.</summary>
    public QuestionsViewModel(QuizCraftDbContext context, IQuestionRepository questionRepository)
    {
        _context = context;
        _questionRepository = questionRepository;
    }

    /// <summary>Carrega matérias e executa a busca inicial ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        await LoadSubjectsAsync();
        await SearchQuestionsAsync();
    }

    /// <summary>Carrega todas as matérias e tópicos para os filtros e editor.</summary>
    private async Task LoadSubjectsAsync()
    {
        var subjects = await _context.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        Subjects.Clear();
        foreach (var s in subjects) Subjects.Add(s);

        var topics = await _context.Topics
            .Include(t => t.Subject)
            .OrderBy(t => t.Subject.Name).ThenBy(t => t.Name)
            .AsNoTracking().ToListAsync();
        AllTopics.Clear();
        foreach (var t in topics) AllTopics.Add(t);
    }

    /// <summary>Busca questões com os filtros aplicados e paginação.</summary>
    [RelayCommand]
    private async Task SearchQuestionsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            const int pageSize = 50;
            var questions = await _questionRepository.SearchAsync(
                string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                FilterTopic?.Id, FilterSubject?.Id, FilterDifficulty, null,
                CurrentPage, pageSize);

            TotalCount = await _questionRepository.SearchCountAsync(
                string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                FilterTopic?.Id, FilterSubject?.Id, FilterDifficulty, null);

            // Calcula o total de páginas com base no tamanho da página
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)pageSize));

            Questions.Clear();
            foreach (var q in questions) Questions.Add(q);
        });
    }

    /// <summary>Avança para a próxima página de resultados.</summary>
    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            _ = SearchQuestionsAsync();
        }
    }

    /// <summary>Volta para a página anterior de resultados.</summary>
    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            _ = SearchQuestionsAsync();
        }
    }

    /// <summary>Abre o editor para criar uma nova questão com 4 alternativas padrão.</summary>
    [RelayCommand]
    private void NewQuestion()
    {
        IsEditorOpen = true;
        IsNewQuestion = true;
        EditingQuestionId = 0;
        EditorStatement = string.Empty;
        EditorExplanation = null;
        EditorType = QuestionType.MultipleChoice;
        EditorDifficulty = 1;
        EditorSource = null;
        EditorTopic = null;
        EditorTagsText = string.Empty;

        // Cria 4 alternativas padrão, a primeira marcada como correta
        EditorChoices.Clear();
        for (int i = 0; i < 4; i++)
            EditorChoices.Add(new EditableChoice { Order = i, IsCorrect = i == 0 });
    }

    /// <summary>Abre o editor para editar uma questão existente, carregando seus dados.</summary>
    [RelayCommand]
    private async Task EditQuestionAsync(Question? question)
    {
        if (question == null) return;

        // Busca a questão com detalhes (alternativas, tags)
        var q = await _questionRepository.GetWithDetailsAsync(question.Id);
        if (q == null) return;

        IsEditorOpen = true;
        IsNewQuestion = false;
        EditingQuestionId = q.Id;
        EditorStatement = q.Statement;
        EditorExplanation = q.Explanation;
        EditorType = q.Type;
        EditorDifficulty = q.Difficulty;
        EditorSource = q.Source;
        EditorTopic = AllTopics.FirstOrDefault(t => t.Id == q.TopicId);
        EditorTagsText = string.Join(", ", q.QuestionTags.Select(qt => qt.Tag.Name));

        EditorChoices.Clear();
        foreach (var c in q.Choices.OrderBy(c => c.Order))
            EditorChoices.Add(new EditableChoice { Id = c.Id, Text = c.Text, IsCorrect = c.IsCorrect, Order = c.Order });
    }

    /// <summary>Salva a questão (nova ou editada) com alternativas e tags.</summary>
    [RelayCommand]
    private async Task SaveQuestionAsync()
    {
        // Validação: tópico e enunciado são obrigatórios
        if (EditorTopic == null || string.IsNullOrWhiteSpace(EditorStatement)) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            Question question;
            if (IsNewQuestion)
            {
                question = new Question
                {
                    TopicId = EditorTopic.Id,
                    Type = EditorType,
                    Statement = EditorStatement,
                    Explanation = EditorExplanation,
                    Difficulty = EditorDifficulty,
                    Source = EditorSource
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
            }
            else
            {
                question = await _context.Questions
                    .Include(q => q.Choices)
                    .Include(q => q.QuestionTags)
                    .FirstAsync(q => q.Id == EditingQuestionId);

                question.TopicId = EditorTopic.Id;
                question.Type = EditorType;
                question.Statement = EditorStatement;
                question.Explanation = EditorExplanation;
                question.Difficulty = EditorDifficulty;
                question.Source = EditorSource;
                question.UpdatedAt = DateTime.UtcNow;

                // Remove alternativas e tags antigas antes de recriar
                _context.Choices.RemoveRange(question.Choices);
                _context.QuestionTags.RemoveRange(question.QuestionTags);
            }

            // Adiciona alternativas não vazias
            int order = 0;
            foreach (var ec in EditorChoices.Where(c => !string.IsNullOrWhiteSpace(c.Text)))
            {
                _context.Choices.Add(new Choice
                {
                    QuestionId = question.Id,
                    Text = ec.Text,
                    IsCorrect = ec.IsCorrect,
                    Order = order++
                });
            }

            // Processa e adiciona tags (cria novas se não existirem)
            if (!string.IsNullOrWhiteSpace(EditorTagsText))
            {
                var tagNames = EditorTagsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }
                    _context.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tag.Id });
                }
            }

            await _context.SaveChangesAsync();
            IsEditorOpen = false;
            await SearchQuestionsAsync();
        });
    }

    /// <summary>Exclui uma questão do banco de dados.</summary>
    [RelayCommand]
    private async Task DeleteQuestionAsync(Question? question)
    {
        if (question == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var entity = await _context.Questions.FindAsync(question.Id);
            if (entity == null) return;

            _context.Questions.Remove(entity);
            await _context.SaveChangesAsync();
            await SearchQuestionsAsync();
        });
    }

    /// <summary>Adiciona uma nova alternativa ao editor (máximo 6).</summary>
    [RelayCommand]
    private void AddChoice()
    {
        if (EditorChoices.Count < 6)
            EditorChoices.Add(new EditableChoice { Order = EditorChoices.Count });
    }

    /// <summary>Remove uma alternativa do editor (mínimo 2).</summary>
    [RelayCommand]
    private void RemoveChoice(EditableChoice? choice)
    {
        if (choice != null && EditorChoices.Count > 2)
            EditorChoices.Remove(choice);
    }

    /// <summary>Fecha o painel de edição de questão.</summary>
    [RelayCommand]
    private void CloseEditor()
    {
        IsEditorOpen = false;
    }
}

/// <summary>
/// Modelo editável de alternativa usado no editor de questões.
/// </summary>
public partial class EditableChoice : ObservableObject
{
    /// <summary>ID da alternativa (0 para novas).</summary>
    public int Id { get; set; }

    /// <summary>Texto da alternativa.</summary>
    [ObservableProperty] private string _text = string.Empty;
    /// <summary>Indica se esta é a alternativa correta.</summary>
    [ObservableProperty] private bool _isCorrect;
    /// <summary>Ordem de exibição da alternativa.</summary>
    [ObservableProperty] private int _order;
}
