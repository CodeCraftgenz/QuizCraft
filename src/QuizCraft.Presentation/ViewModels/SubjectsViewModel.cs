using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Infrastructure.Data;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel de gerenciamento de matérias e tópicos. Permite CRUD completo de matérias e seus tópicos/subtópicos.
/// </summary>
public partial class SubjectsViewModel : BaseViewModel
{
    private readonly QuizCraftDbContext _context;

    /// <summary>Nome da nova matéria a ser criada.</summary>
    [ObservableProperty] private string _newSubjectName = string.Empty;
    /// <summary>Descrição opcional da nova matéria.</summary>
    [ObservableProperty] private string? _newSubjectDescription;
    /// <summary>Cor hexadecimal da nova matéria (padrão azul).</summary>
    [ObservableProperty] private string _newSubjectColor = "#2196F3";
    /// <summary>Matéria atualmente selecionada na lista.</summary>
    [ObservableProperty] private Subject? _selectedSubject;
    /// <summary>Nome do novo tópico a ser criado.</summary>
    [ObservableProperty] private string _newTopicName = string.Empty;
    /// <summary>Tópico atualmente selecionado (usado como pai para subtópicos).</summary>
    [ObservableProperty] private Topic? _selectedTopic;
    /// <summary>Indica se o modo de edição de matéria está ativo.</summary>
    [ObservableProperty] private bool _isEditing;
    /// <summary>Indica se o modo de edição de tópico está ativo.</summary>
    [ObservableProperty] private bool _isEditingTopic;

    /// <summary>Lista observável de matérias carregadas.</summary>
    public ObservableCollection<Subject> Subjects { get; } = new();
    /// <summary>Lista observável de tópicos da matéria selecionada.</summary>
    public ObservableCollection<Topic> Topics { get; } = new();

    /// <summary>Inicializa o ViewModel com o contexto do banco de dados.</summary>
    public SubjectsViewModel(QuizCraftDbContext context)
    {
        _context = context;
    }

    /// <summary>Carrega a lista de matérias ao inicializar.</summary>
    public override async Task InitializeAsync()
    {
        await LoadSubjectsAsync();
    }

    /// <summary>Carrega todas as matérias com seus tópicos raiz e subtópicos.</summary>
    [RelayCommand]
    private async Task LoadSubjectsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            // Carrega matérias com tópicos raiz (sem pai) e seus subtópicos
            var subjects = await _context.Subjects
                .Include(s => s.Topics.Where(t => t.ParentTopicId == null))
                    .ThenInclude(t => t.SubTopics)
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            Subjects.Clear();
            foreach (var s in subjects) Subjects.Add(s);
        });
    }

    /// <summary>Adiciona uma nova matéria ao banco de dados.</summary>
    [RelayCommand]
    private async Task AddSubjectAsync()
    {
        // Validação: nome obrigatório
        if (string.IsNullOrWhiteSpace(NewSubjectName)) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var subject = new Subject
            {
                Name = NewSubjectName.Trim(),
                Description = NewSubjectDescription?.Trim(),
                Color = NewSubjectColor
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            // Limpa os campos e recarrega a lista
            NewSubjectName = string.Empty;
            NewSubjectDescription = null;
            await LoadSubjectsAsync();
        });
    }

    /// <summary>Atualiza os dados da matéria selecionada.</summary>
    [RelayCommand]
    private async Task UpdateSubjectAsync()
    {
        if (SelectedSubject == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var subject = await _context.Subjects.FindAsync(SelectedSubject.Id);
            if (subject == null) return;

            // Atualiza os campos da matéria
            subject.Name = SelectedSubject.Name;
            subject.Description = SelectedSubject.Description;
            subject.Color = SelectedSubject.Color;
            subject.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            IsEditing = false;
            await LoadSubjectsAsync();
        });
    }

    /// <summary>Remove uma matéria e todos os seus dados relacionados.</summary>
    [RelayCommand]
    private async Task DeleteSubjectAsync(Subject? subject)
    {
        if (subject == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var entity = await _context.Subjects.FindAsync(subject.Id);
            if (entity == null) return;

            _context.Subjects.Remove(entity);
            await _context.SaveChangesAsync();
            await LoadSubjectsAsync();
        });
    }

    /// <summary>Reage à mudança de matéria selecionada, carregando seus tópicos.</summary>
    partial void OnSelectedSubjectChanged(Subject? value)
    {
        if (value != null)
        {
            // Carrega os tópicos da matéria selecionada
            _ = LoadTopicsAsync(value.Id);
        }
        else
        {
            Topics.Clear();
        }
    }

    /// <summary>Carrega os tópicos raiz de uma matéria específica.</summary>
    [RelayCommand]
    private async Task LoadTopicsAsync(int subjectId)
    {
        // Busca apenas tópicos raiz (sem pai) com seus subtópicos
        var topics = await _context.Topics
            .Where(t => t.SubjectId == subjectId && t.ParentTopicId == null)
            .Include(t => t.SubTopics)
            .OrderBy(t => t.Name)
            .AsNoTracking()
            .ToListAsync();

        Topics.Clear();
        foreach (var t in topics) Topics.Add(t);
    }

    /// <summary>Adiciona um novo tópico à matéria selecionada (pode ser subtópico).</summary>
    [RelayCommand]
    private async Task AddTopicAsync()
    {
        // Validação: matéria selecionada e nome obrigatório
        if (SelectedSubject == null || string.IsNullOrWhiteSpace(NewTopicName)) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var topic = new Topic
            {
                SubjectId = SelectedSubject.Id,
                Name = NewTopicName.Trim(),
                // Se houver tópico selecionado, cria como subtópico
                ParentTopicId = SelectedTopic?.Id
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            NewTopicName = string.Empty;
            await LoadTopicsAsync(SelectedSubject.Id);
        });
    }

    /// <summary>Remove um tópico e recarrega a lista.</summary>
    [RelayCommand]
    private async Task DeleteTopicAsync(Topic? topic)
    {
        if (topic == null || SelectedSubject == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var entity = await _context.Topics.FindAsync(topic.Id);
            if (entity == null) return;

            _context.Topics.Remove(entity);
            await _context.SaveChangesAsync();
            await LoadTopicsAsync(SelectedSubject.Id);
        });
    }
}
