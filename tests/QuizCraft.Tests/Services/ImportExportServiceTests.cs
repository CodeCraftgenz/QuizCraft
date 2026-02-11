using System.Text.Json;
using QuizCraft.Application.Services;
using QuizCraft.Tests.Helpers;

namespace QuizCraft.Tests.Services;

public class ImportExportServiceTests
{
    [Fact]
    public async Task ImportQuestionsJsonAsync_ImportsCorrectly()
    {
        using var context = TestDbContextFactory.Create();
        var service = new ImportExportService(context);

        var json = JsonSerializer.Serialize(new[]
        {
            new QuestionExportDto
            {
                Subject = "Matemática",
                Topic = "Álgebra",
                Type = "MultipleChoice",
                Statement = "Qual é 2 + 2?",
                Explanation = "Soma simples",
                Difficulty = 1,
                Tags = ["básico", "soma"],
                Choices =
                [
                    new ChoiceExportDto { Text = "3", IsCorrect = false },
                    new ChoiceExportDto { Text = "4", IsCorrect = true },
                    new ChoiceExportDto { Text = "5", IsCorrect = false }
                ]
            }
        });

        var count = await service.ImportQuestionsJsonAsync(json);

        Assert.Equal(1, count);
        Assert.Single(context.Questions);
        Assert.Equal("Qual é 2 + 2?", context.Questions.First().Statement);
        Assert.Equal(3, context.Choices.Count());
        Assert.Equal(2, context.Tags.Count());
    }

    [Fact]
    public async Task ImportQuestionsJsonAsync_CreatesSubjectAndTopic()
    {
        using var context = TestDbContextFactory.Create();
        var service = new ImportExportService(context);

        var json = JsonSerializer.Serialize(new[]
        {
            new QuestionExportDto
            {
                Subject = "Física",
                Topic = "Mecânica",
                Statement = "O que é inércia?",
                Difficulty = 2,
                Choices = [new ChoiceExportDto { Text = "Resposta", IsCorrect = true }]
            }
        });

        await service.ImportQuestionsJsonAsync(json);

        Assert.Single(context.Subjects);
        Assert.Equal("Física", context.Subjects.First().Name);
        Assert.Single(context.Topics);
        Assert.Equal("Mecânica", context.Topics.First().Name);
    }

    [Fact]
    public async Task ImportQuestionsJsonAsync_ReuseExistingSubjectAndTopic()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context, "Matemática");
        var topic = TestDataBuilder.CreateTopic(context, subject.Id, "Álgebra");
        var service = new ImportExportService(context);

        var json = JsonSerializer.Serialize(new[]
        {
            new QuestionExportDto
            {
                Subject = "Matemática",
                Topic = "Álgebra",
                Statement = "Questão nova",
                Difficulty = 1,
                Choices = [new ChoiceExportDto { Text = "A", IsCorrect = true }]
            }
        });

        await service.ImportQuestionsJsonAsync(json);

        Assert.Single(context.Subjects);
        Assert.Single(context.Topics);
        Assert.Single(context.Questions);
    }

    [Fact]
    public async Task ImportQuestionsJsonAsync_EmptyJson_ReturnsZero()
    {
        using var context = TestDbContextFactory.Create();
        var service = new ImportExportService(context);

        var count = await service.ImportQuestionsJsonAsync("[]");

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task ImportQuestionsJsonAsync_ClampsDifficulty()
    {
        using var context = TestDbContextFactory.Create();
        var service = new ImportExportService(context);

        var json = JsonSerializer.Serialize(new[]
        {
            new QuestionExportDto
            {
                Subject = "X", Topic = "Y", Statement = "Q1", Difficulty = 10,
                Choices = [new ChoiceExportDto { Text = "A", IsCorrect = true }]
            },
            new QuestionExportDto
            {
                Subject = "X", Topic = "Y", Statement = "Q2", Difficulty = -1,
                Choices = [new ChoiceExportDto { Text = "B", IsCorrect = true }]
            }
        });

        await service.ImportQuestionsJsonAsync(json);

        var questions = context.Questions.ToList();
        Assert.Equal(5, questions[0].Difficulty);
        Assert.Equal(1, questions[1].Difficulty);
    }

    [Fact]
    public async Task ExportQuestionsJsonAsync_ExportsAll()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");
        TestDataBuilder.CreateQuestion(context, topic.Id, "Q2");

        var service = new ImportExportService(context);
        var json = await service.ExportQuestionsJsonAsync();

        var data = JsonSerializer.Deserialize<List<QuestionExportDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(data);
        Assert.Equal(2, data.Count);
    }

    [Fact]
    public async Task ExportQuestionsCsvAsync_IncludesHeader()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        TestDataBuilder.CreateQuestion(context, topic.Id);

        var service = new ImportExportService(context);
        var csv = await service.ExportQuestionsCsvAsync();

        Assert.StartsWith("Subject;Topic;Type;Statement", csv);
        var lines = csv.Split(Environment.NewLine);
        Assert.Equal(2, lines.Length); // Header + 1 data line
    }

    [Fact]
    public async Task ExportStatsCsvAsync_IncludesCompletedSessions()
    {
        using var context = TestDbContextFactory.Create();
        TestDataBuilder.CreateSession(context, totalQuestions: 5, correctCount: 3);
        TestDataBuilder.CreateSession(context, status: Domain.Enums.SessionStatus.InProgress); // Should not appear

        var service = new ImportExportService(context);
        var csv = await service.ExportStatsCsvAsync();

        var lines = csv.Split(Environment.NewLine);
        Assert.Equal(2, lines.Length); // Header + 1 completed session
    }
}
