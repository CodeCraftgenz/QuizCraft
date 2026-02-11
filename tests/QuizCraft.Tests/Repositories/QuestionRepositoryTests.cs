using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Infrastructure.Repositories;
using QuizCraft.Tests.Helpers;

namespace QuizCraft.Tests.Repositories;

public class QuestionRepositoryTests
{
    [Fact]
    public async Task SearchAsync_ByText_ReturnsMatches()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        TestDataBuilder.CreateQuestion(context, topic.Id, "Qual é a raiz de 4?");
        TestDataBuilder.CreateQuestion(context, topic.Id, "Defina logaritmo.");

        var repo = new QuestionRepository(context);
        var results = await repo.SearchAsync("raiz", null, null, null, null);

        Assert.Single(results);
        Assert.Contains("raiz", results[0].Statement);
    }

    [Fact]
    public async Task SearchAsync_ByTopic_ReturnsFiltered()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic1 = TestDataBuilder.CreateTopic(context, subject.Id, "Álgebra");
        var topic2 = TestDataBuilder.CreateTopic(context, subject.Id, "Geometria");
        TestDataBuilder.CreateQuestion(context, topic1.Id, "Q Alg");
        TestDataBuilder.CreateQuestion(context, topic2.Id, "Q Geo");

        var repo = new QuestionRepository(context);
        var results = await repo.SearchAsync(null, topic1.Id, null, null, null);

        Assert.Single(results);
    }

    [Fact]
    public async Task SearchCountAsync_ReturnsTotal()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        for (int i = 0; i < 15; i++)
            TestDataBuilder.CreateQuestion(context, topic.Id, $"Q{i}");

        var repo = new QuestionRepository(context);
        var count = await repo.SearchCountAsync(null, null, null, null, null);

        Assert.Equal(15, count);
    }

    [Fact]
    public async Task SearchAsync_Pagination_Works()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        for (int i = 0; i < 10; i++)
            TestDataBuilder.CreateQuestion(context, topic.Id, $"Q{i}");

        var repo = new QuestionRepository(context);
        var page1 = await repo.SearchAsync(null, null, null, null, null, page: 1, pageSize: 3);
        var page2 = await repo.SearchAsync(null, null, null, null, null, page: 2, pageSize: 3);

        Assert.Equal(3, page1.Count);
        Assert.Equal(3, page2.Count);
        Assert.NotEqual(page1[0].Id, page2[0].Id);
    }

    [Fact]
    public async Task GetWithDetailsAsync_IncludesAllRelations()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var question = TestDataBuilder.CreateQuestion(context, topic.Id);

        var tag = new Tag { Name = "teste" };
        context.Tags.Add(tag);
        context.SaveChanges();
        context.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tag.Id });
        context.SaveChanges();

        var repo = new QuestionRepository(context);
        var result = await repo.GetWithDetailsAsync(question.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.Topic);
        Assert.NotEmpty(result.Choices);
        Assert.NotEmpty(result.QuestionTags);
    }

    [Fact]
    public async Task GetForQuizAsync_RespectsCount()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        for (int i = 0; i < 20; i++)
            TestDataBuilder.CreateQuestion(context, topic.Id, $"Q{i}");

        var repo = new QuestionRepository(context);
        var questions = await repo.GetForQuizAsync(null, null, null, null, null, 5, false);

        Assert.Equal(5, questions.Count);
    }

    [Fact]
    public async Task GetForQuizAsync_FiltersByDifficulty()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        TestDataBuilder.CreateQuestion(context, topic.Id, "Easy", difficulty: 1);
        TestDataBuilder.CreateQuestion(context, topic.Id, "Medium", difficulty: 3);
        TestDataBuilder.CreateQuestion(context, topic.Id, "Hard", difficulty: 5);

        var repo = new QuestionRepository(context);
        var questions = await repo.GetForQuizAsync(null, null, null, 3, 5, 10, false);

        Assert.Equal(2, questions.Count);
        Assert.All(questions, q => Assert.True(q.Difficulty >= 3));
    }

    [Fact]
    public async Task GetDueForReviewAsync_ReturnsDueQuestions()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var q1 = TestDataBuilder.CreateQuestion(context, topic.Id, "Due Q");
        var q2 = TestDataBuilder.CreateQuestion(context, topic.Id, "Not Due Q");

        context.Masteries.AddRange(
            new Mastery { QuestionId = q1.Id, Level = 1, NextReviewAt = DateTime.UtcNow.AddDays(-1) },
            new Mastery { QuestionId = q2.Id, Level = 3, NextReviewAt = DateTime.UtcNow.AddDays(10) }
        );
        context.SaveChanges();

        var repo = new QuestionRepository(context);
        var due = await repo.GetDueForReviewAsync(DateTime.UtcNow, null, 10);

        Assert.Single(due);
        Assert.Equal(q1.Id, due[0].Id);
    }

    [Fact]
    public async Task GetByTopicAsync_ReturnsOnlyTopicQuestions()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic1 = TestDataBuilder.CreateTopic(context, subject.Id, "A");
        var topic2 = TestDataBuilder.CreateTopic(context, subject.Id, "B");
        TestDataBuilder.CreateQuestion(context, topic1.Id, "Q1");
        TestDataBuilder.CreateQuestion(context, topic1.Id, "Q2");
        TestDataBuilder.CreateQuestion(context, topic2.Id, "Q3");

        var repo = new QuestionRepository(context);
        var results = await repo.GetByTopicAsync(topic1.Id);

        Assert.Equal(2, results.Count);
    }
}
