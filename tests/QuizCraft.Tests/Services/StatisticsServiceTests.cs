using QuizCraft.Application.Services;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Tests.Helpers;

namespace QuizCraft.Tests.Services;

public class StatisticsServiceTests
{
    [Fact]
    public async Task GetDashboardStatsAsync_EmptyDatabase_ReturnsZeros()
    {
        using var context = TestDbContextFactory.Create();
        var service = new StatisticsService(context);

        var stats = await service.GetDashboardStatsAsync();

        Assert.Equal(0, stats.TotalQuestions);
        Assert.Equal(0, stats.QuestionsStudied);
        Assert.Equal(0, stats.AccuracyRate7Days);
        Assert.Equal(0, stats.AccuracyRate30Days);
        Assert.Equal(0, stats.TotalSessions);
        Assert.Equal(0, stats.CurrentStreak);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithData_ReturnsCorrectStats()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var q1 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");
        var q2 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q2");
        var q3 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q3");

        var session = TestDataBuilder.CreateSession(context, totalQuestions: 3, correctCount: 2);
        TestDataBuilder.CreateSessionItem(context, session.Id, q1.Id, isCorrect: true, order: 0);
        TestDataBuilder.CreateSessionItem(context, session.Id, q2.Id, isCorrect: true, order: 1);
        TestDataBuilder.CreateSessionItem(context, session.Id, q3.Id, isCorrect: false, order: 2);

        var service = new StatisticsService(context);
        var stats = await service.GetDashboardStatsAsync();

        Assert.Equal(3, stats.TotalQuestions);
        Assert.Equal(3, stats.QuestionsStudied);
        Assert.Equal(1, stats.TotalSessions);
        Assert.True(stats.AccuracyRate7Days > 60);
    }

    [Fact]
    public async Task RecordStudyDayAsync_CreatesNewEntry()
    {
        using var context = TestDbContextFactory.Create();
        var service = new StatisticsService(context);

        await service.RecordStudyDayAsync(10, 7, 300);

        var streak = context.StudyStreaks.FirstOrDefault();
        Assert.NotNull(streak);
        Assert.Equal(10, streak.QuestionsAnswered);
        Assert.Equal(7, streak.CorrectAnswers);
        Assert.Equal(300, streak.StudyTimeSeconds);
    }

    [Fact]
    public async Task RecordStudyDayAsync_UpdatesExisting_SameDay()
    {
        using var context = TestDbContextFactory.Create();
        var service = new StatisticsService(context);

        await service.RecordStudyDayAsync(10, 7, 300);
        await service.RecordStudyDayAsync(5, 3, 150);

        var streaks = context.StudyStreaks.ToList();
        Assert.Single(streaks);
        Assert.Equal(15, streaks[0].QuestionsAnswered);
        Assert.Equal(10, streaks[0].CorrectAnswers);
        Assert.Equal(450, streaks[0].StudyTimeSeconds);
    }

    [Fact]
    public async Task GetWeakestTopicsAsync_ReturnsLowestAccuracy()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic1 = TestDataBuilder.CreateTopic(context, subject.Id, "Álgebra");
        var topic2 = TestDataBuilder.CreateTopic(context, subject.Id, "Geometria");

        var q1 = TestDataBuilder.CreateQuestion(context, topic1.Id, "Q Algebra");
        var q2 = TestDataBuilder.CreateQuestion(context, topic2.Id, "Q Geometria");

        var session = TestDataBuilder.CreateSession(context, totalQuestions: 6, correctCount: 4);

        // Álgebra: 3 correct, 0 incorrect -> 100%
        TestDataBuilder.CreateSessionItem(context, session.Id, q1.Id, isCorrect: true, order: 0);
        TestDataBuilder.CreateSessionItem(context, session.Id, q1.Id, isCorrect: true, order: 1);
        TestDataBuilder.CreateSessionItem(context, session.Id, q1.Id, isCorrect: true, order: 2);

        // Geometria: 1 correct, 2 incorrect -> 33%
        TestDataBuilder.CreateSessionItem(context, session.Id, q2.Id, isCorrect: true, order: 3);
        TestDataBuilder.CreateSessionItem(context, session.Id, q2.Id, isCorrect: false, order: 4);
        TestDataBuilder.CreateSessionItem(context, session.Id, q2.Id, isCorrect: false, order: 5);

        var service = new StatisticsService(context);
        var weak = await service.GetWeakestTopicsAsync(5);

        Assert.NotEmpty(weak);
        Assert.Equal("Geometria", weak[0].TopicName);
    }

    [Fact]
    public async Task GetDailyPerformanceAsync_ReturnsGroupedByDate()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var q1 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");

        var session = new QuizSession
        {
            Mode = QuizMode.Training,
            Status = SessionStatus.Completed,
            TotalQuestions = 2,
            CorrectCount = 1,
            DurationSeconds = 60,
            StartedAt = DateTime.UtcNow.AddDays(-1),
            EndedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(1)
        };
        context.QuizSessions.Add(session);
        context.SaveChanges();

        context.QuizSessionItems.AddRange(
            new QuizSessionItem { SessionId = session.Id, QuestionId = q1.Id, IsCorrect = true, TimeSeconds = 10, Order = 0 },
            new QuizSessionItem { SessionId = session.Id, QuestionId = q1.Id, IsCorrect = false, TimeSeconds = 10, Order = 1 }
        );
        context.SaveChanges();

        var service = new StatisticsService(context);
        var daily = await service.GetDailyPerformanceAsync(7);

        Assert.NotEmpty(daily);
        Assert.Equal(2, daily[0].QuestionsAnswered);
        Assert.Equal(50.0, daily[0].AccuracyRate);
    }
}
