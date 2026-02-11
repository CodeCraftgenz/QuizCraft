using QuizCraft.Application.Services;
using QuizCraft.Domain.Entities;
using QuizCraft.Infrastructure.Repositories;
using QuizCraft.Tests.Helpers;

namespace QuizCraft.Tests.Services;

public class SpacedRepetitionServiceTests
{
    [Theory]
    [InlineData(0, true, 1)]
    [InlineData(1, true, 2)]
    [InlineData(4, true, 5)]
    [InlineData(5, true, 5)]  // Cap at 5
    [InlineData(5, false, 3)]
    [InlineData(3, false, 1)]
    [InlineData(1, false, 0)]
    [InlineData(0, false, 0)] // Floor at 0
    public void CalculateNewLevel_ReturnsCorrectLevel(int currentLevel, bool wasCorrect, int expected)
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        var result = service.CalculateNewLevel(currentLevel, wasCorrect);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateNextReview_WhenCorrect_UsesStandardInterval()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        var before = DateTime.UtcNow;
        var result = service.CalculateNextReview(3, wasCorrect: true);

        // Level 3 has interval of 7 days
        Assert.True(result > before.AddDays(6.9));
        Assert.True(result < before.AddDays(7.1));
    }

    [Fact]
    public void CalculateNextReview_WhenIncorrect_UsesHalfInterval()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        var before = DateTime.UtcNow;
        var result = service.CalculateNextReview(3, wasCorrect: false);

        // Level 3 interval = 7 days, halved = 3.5 days
        Assert.True(result > before.AddDays(3.4));
        Assert.True(result < before.AddDays(3.6));
    }

    [Fact]
    public void CalculateNextReview_Level0_Correct_ReturnsNow()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        var before = DateTime.UtcNow;
        var result = service.CalculateNextReview(0, wasCorrect: true);

        // Level 0 has interval of 0 days
        Assert.True(result >= before.AddMinutes(-1));
        Assert.True(result <= before.AddMinutes(1));
    }

    [Fact]
    public async Task UpdateMasteryAsync_CreatesNewMastery_WhenNoneExists()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var question = TestDataBuilder.CreateQuestion(context, topic.Id);
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        await service.UpdateMasteryAsync(question.Id, true, 10);

        var mastery = context.Masteries.FirstOrDefault(m => m.QuestionId == question.Id);
        Assert.NotNull(mastery);
        Assert.Equal(1, mastery.Level);
        Assert.Equal(1, mastery.TotalAttempts);
        Assert.Equal(1, mastery.TotalCorrect);
        Assert.Equal(1, mastery.RightStreak);
        Assert.Equal(0, mastery.WrongStreak);
    }

    [Fact]
    public async Task UpdateMasteryAsync_IncrementsWrongStreak_WhenIncorrect()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var question = TestDataBuilder.CreateQuestion(context, topic.Id);
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        await service.UpdateMasteryAsync(question.Id, false, 10);

        var mastery = context.Masteries.First(m => m.QuestionId == question.Id);
        Assert.Equal(0, mastery.Level);
        Assert.Equal(1, mastery.WrongStreak);
        Assert.Equal(0, mastery.RightStreak);
        Assert.Equal(0, mastery.TotalCorrect);
    }

    [Fact]
    public async Task UpdateMasteryAsync_UpdatesExistingMastery()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var question = TestDataBuilder.CreateQuestion(context, topic.Id);
        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        await service.UpdateMasteryAsync(question.Id, true, 5);
        await service.UpdateMasteryAsync(question.Id, true, 8);
        await service.UpdateMasteryAsync(question.Id, false, 12);

        var mastery = context.Masteries.First(m => m.QuestionId == question.Id);
        Assert.Equal(3, mastery.TotalAttempts);
        Assert.Equal(2, mastery.TotalCorrect);
        Assert.Equal(1, mastery.WrongStreak);
        Assert.Equal(0, mastery.RightStreak);
        // Level: 0 -> 1 -> 2 -> 0 (correct, correct, incorrect drops 2)
        Assert.Equal(0, mastery.Level);
    }

    [Fact]
    public async Task GetDueCountAsync_ReturnsCorrectCount()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var q1 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");
        var q2 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q2");
        var q3 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q3");

        context.Masteries.AddRange(
            new Mastery { QuestionId = q1.Id, Level = 1, NextReviewAt = DateTime.UtcNow.AddDays(-1) },
            new Mastery { QuestionId = q2.Id, Level = 2, NextReviewAt = DateTime.UtcNow.AddDays(-2) },
            new Mastery { QuestionId = q3.Id, Level = 3, NextReviewAt = DateTime.UtcNow.AddDays(5) } // Not due
        );
        context.SaveChanges();

        var repo = new QuestionRepository(context);
        var service = new SpacedRepetitionService(context, repo);

        var count = await service.GetDueCountAsync();

        Assert.Equal(2, count);
    }
}
