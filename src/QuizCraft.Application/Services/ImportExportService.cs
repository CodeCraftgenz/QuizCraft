using System.Text.Json;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizCraft.Application.Services;

/// <summary>
/// Servico de importacao e exportacao de questoes nos formatos JSON e CSV.
/// Permite exportar/importar questoes com alternativas, tags e estatisticas.
/// </summary>
public class ImportExportService
{
    private readonly QuizCraftDbContext _context;

    /// <summary>
    /// Inicializa o servico com o contexto do banco de dados.
    /// </summary>
    public ImportExportService(QuizCraftDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Exporta questoes em formato JSON, com filtro opcional por disciplina e topico.
    /// </summary>
    public async Task<string> ExportQuestionsJsonAsync(int? subjectId = null, int? topicId = null)
    {
        var query = _context.Questions
            .Include(q => q.Choices.OrderBy(c => c.Order))
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Topic).ThenInclude(t => t.Subject)
            .AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(q => q.Topic.SubjectId == subjectId.Value);
        if (topicId.HasValue)
            query = query.Where(q => q.TopicId == topicId.Value);

        var questions = await query.AsNoTracking().ToListAsync();

        // Mapeia entidades para DTOs de exportacao
        var exportData = questions.Select(q => new QuestionExportDto
        {
            Subject = q.Topic.Subject.Name,
            Topic = q.Topic.Name,
            Type = q.Type.ToString(),
            Statement = q.Statement,
            Explanation = q.Explanation,
            Difficulty = q.Difficulty,
            Source = q.Source,
            Tags = q.QuestionTags.Select(qt => qt.Tag.Name).ToList(),
            Choices = q.Choices.Select(c => new ChoiceExportDto
            {
                Text = c.Text,
                IsCorrect = c.IsCorrect
            }).ToList()
        }).ToList();

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Exporta questoes em formato CSV (separador ponto-e-virgula), suportando ate 5 alternativas.
    /// </summary>
    public async Task<string> ExportQuestionsCsvAsync(int? subjectId = null, int? topicId = null)
    {
        var query = _context.Questions
            .Include(q => q.Choices.OrderBy(c => c.Order))
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Topic).ThenInclude(t => t.Subject)
            .AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(q => q.Topic.SubjectId == subjectId.Value);
        if (topicId.HasValue)
            query = query.Where(q => q.TopicId == topicId.Value);

        var questions = await query.AsNoTracking().ToListAsync();

        // Cabecalho do CSV
        var lines = new List<string>
        {
            "Subject;Topic;Type;Statement;Explanation;Difficulty;Tags;ChoiceA;ChoiceB;ChoiceC;ChoiceD;ChoiceE;CorrectAnswer"
        };

        foreach (var q in questions)
        {
            var tags = string.Join(",", q.QuestionTags.Select(qt => qt.Tag.Name));
            var choices = q.Choices.OrderBy(c => c.Order).ToList();
            // Identifica o indice da alternativa correta para converter em letra (A-E)
            var correctIdx = choices.FindIndex(c => c.IsCorrect);
            var letters = new[] { "A", "B", "C", "D", "E" };

            var choiceTexts = new string[5];
            for (int i = 0; i < 5; i++)
                choiceTexts[i] = i < choices.Count ? EscapeCsv(choices[i].Text) : "";

            var correct = correctIdx >= 0 && correctIdx < letters.Length ? letters[correctIdx] : "";

            lines.Add($"{EscapeCsv(q.Topic.Subject.Name)};{EscapeCsv(q.Topic.Name)};{q.Type};" +
                       $"{EscapeCsv(q.Statement)};{EscapeCsv(q.Explanation ?? "")};{q.Difficulty};" +
                       $"{tags};{string.Join(";", choiceTexts)};{correct}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Importa questoes a partir de uma string JSON. Cria disciplinas, topicos e tags automaticamente.
    /// </summary>
    /// <param name="json">Conteudo JSON com a lista de questoes.</param>
    /// <returns>Quantidade de questoes importadas com sucesso.</returns>
    public async Task<int> ImportQuestionsJsonAsync(string json)
    {
        var data = JsonSerializer.Deserialize<List<QuestionExportDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (data == null || data.Count == 0) return 0;

        var count = 0;
        foreach (var dto in data)
        {
            // Busca ou cria disciplina e topico correspondentes
            var subject = await GetOrCreateSubjectAsync(dto.Subject);
            var topic = await GetOrCreateTopicAsync(dto.Topic, subject.Id);

            // Converte tipo da questao; padrao e multipla escolha se invalido
            if (!Enum.TryParse<QuestionType>(dto.Type, true, out var qType))
                qType = QuestionType.MultipleChoice;

            var question = new Question
            {
                TopicId = topic.Id,
                Type = qType,
                Statement = dto.Statement,
                Explanation = dto.Explanation,
                Difficulty = Math.Clamp(dto.Difficulty, 1, 5),
                Source = dto.Source,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            int order = 0;
            foreach (var choiceDto in dto.Choices)
            {
                _context.Choices.Add(new Choice
                {
                    QuestionId = question.Id,
                    Text = choiceDto.Text,
                    IsCorrect = choiceDto.IsCorrect,
                    Order = order++
                });
            }

            foreach (var tagName in dto.Tags)
            {
                var tag = await GetOrCreateTagAsync(tagName);
                _context.QuestionTags.Add(new QuestionTag
                {
                    QuestionId = question.Id,
                    TagId = tag.Id
                });
            }

            await _context.SaveChangesAsync();
            count++;
        }

        return count;
    }

    /// <summary>
    /// Exporta o historico de sessoes concluidas em formato CSV.
    /// </summary>
    public async Task<string> ExportStatsCsvAsync()
    {
        var sessions = await _context.QuizSessions
            .Where(s => s.Status == Domain.Enums.SessionStatus.Completed)
            .OrderByDescending(s => s.StartedAt)
            .AsNoTracking()
            .ToListAsync();

        var lines = new List<string>
        {
            "Date;Mode;TotalQuestions;CorrectCount;AccuracyRate;DurationSeconds"
        };

        foreach (var s in sessions)
        {
            var rate = s.TotalQuestions > 0 ? (double)s.CorrectCount / s.TotalQuestions * 100 : 0;
            lines.Add($"{s.StartedAt:yyyy-MM-dd HH:mm};{s.Mode};{s.TotalQuestions};{s.CorrectCount};{rate:F1}%;{s.DurationSeconds}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Busca uma disciplina pelo nome ou cria uma nova caso nao exista.
    /// </summary>
    private async Task<Subject> GetOrCreateSubjectAsync(string name)
    {
        var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == name);
        if (subject != null) return subject;

        subject = new Subject { Name = name, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    /// <summary>
    /// Busca um topico pelo nome e disciplina ou cria um novo caso nao exista.
    /// </summary>
    private async Task<Topic> GetOrCreateTopicAsync(string name, int subjectId)
    {
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == name && t.SubjectId == subjectId);
        if (topic != null) return topic;

        topic = new Topic { Name = name, SubjectId = subjectId, CreatedAt = DateTime.UtcNow };
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    /// <summary>
    /// Busca uma tag pelo nome ou cria uma nova caso nao exista.
    /// </summary>
    private async Task<Tag> GetOrCreateTagAsync(string name)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
        if (tag != null) return tag;

        tag = new Tag { Name = name.Trim() };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    /// <summary>
    /// Escapa valores CSV que contenham separador, aspas ou quebra de linha.
    /// </summary>
    private static string EscapeCsv(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

/// <summary>
/// DTO para exportacao/importacao de questoes.
/// </summary>
public class QuestionExportDto
{
    public string Subject { get; set; } = "";
    public string Topic { get; set; } = "";
    public string Type { get; set; } = "MultipleChoice";
    public string Statement { get; set; } = "";
    public string? Explanation { get; set; }
    public int Difficulty { get; set; } = 1;
    public string? Source { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<ChoiceExportDto> Choices { get; set; } = [];
}

/// <summary>
/// DTO para exportacao/importacao de alternativas.
/// </summary>
public class ChoiceExportDto
{
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}
