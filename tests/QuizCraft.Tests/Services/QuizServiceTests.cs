using Microsoft.EntityFrameworkCore;
using NSubstitute;
using QuizCraft.Application.Services;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Repositories;
using QuizCraft.Tests.Helpers;

namespace QuizCraft.Tests.Services;

public class QuizServiceTests
{
    [Fact]
    public async Task StartSessionAsync_CreatesSession()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var session = await quizService.StartSessionAsync(QuizMode.Training, 10, null, null);

        Assert.True(session.Id > 0);
        Assert.Equal(QuizMode.Training, session.Mode);
        Assert.Equal(SessionStatus.InProgress, session.Status);
        Assert.Equal(10, session.TotalQuestions);
    }

    [Fact]
    public async Task StartSessionAsync_WithTimeLimit_SetsLimit()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var session = await quizService.StartSessionAsync(QuizMode.Exam, 20, 3600, "{\"subjectId\":1}");

        Assert.Equal(3600, session.TimeLimitSeconds);
        Assert.Equal("{\"subjectId\":1}", session.FiltersJson);
    }

    [Fact]
    public async Task RecordAnswerAsync_SavesItemAndUpdatesMastery()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var question = TestDataBuilder.CreateQuestion(context, topic.Id);
        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var session = await quizService.StartSessionAsync(QuizMode.Training, 1, null, null);
        await quizService.RecordAnswerAsync(session.Id, question.Id, "\"4\"", true, 15, false, 0);

        var item = await context.QuizSessionItems.FirstOrDefaultAsync(i => i.SessionId == session.Id);
        Assert.NotNull(item);
        Assert.True(item.IsCorrect);
        Assert.Equal(15, item.TimeSeconds);

        var mastery = await context.Masteries.FirstOrDefaultAsync(m => m.QuestionId == question.Id);
        Assert.NotNull(mastery);
        Assert.Equal(1, mastery.TotalAttempts);
    }

    [Fact]
    public async Task FinishSessionAsync_CompletesSession()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var q1 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");
        var q2 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q2");
        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var session = await quizService.StartSessionAsync(QuizMode.Training, 2, null, null);
        await quizService.RecordAnswerAsync(session.Id, q1.Id, "\"4\"", true, 10, false, 0);
        await quizService.RecordAnswerAsync(session.Id, q2.Id, "\"3\"", false, 8, false, 1);

        var finished = await quizService.FinishSessionAsync(session.Id);

        Assert.Equal(SessionStatus.Completed, finished.Status);
        Assert.Equal(1, finished.CorrectCount);
        Assert.NotNull(finished.EndedAt);
        Assert.True(finished.DurationSeconds >= 0);
    }

    [Fact]
    public async Task FinishSessionAsync_RecordsStudyDay()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        var q1 = TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");
        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var session = await quizService.StartSessionAsync(QuizMode.Training, 1, null, null);
        await quizService.RecordAnswerAsync(session.Id, q1.Id, "\"4\"", true, 10, false, 0);
        await quizService.FinishSessionAsync(session.Id);

        var streak = await context.StudyStreaks.FirstOrDefaultAsync();
        Assert.NotNull(streak);
        Assert.Equal(1, streak.QuestionsAnswered);
        Assert.Equal(1, streak.CorrectAnswers);
    }

    [Fact]
    public async Task FinishSessionAsync_ThrowsForInvalidSession()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => quizService.FinishSessionAsync(999));
    }

    [Fact]
    public async Task BuildQuizQuestionsAsync_Training_ReturnsQuestions()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        for (int i = 0; i < 10; i++)
            TestDataBuilder.CreateQuestion(context, topic.Id, $"Question {i}", difficulty: (i % 5) + 1);

        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var questions = await quizService.BuildQuizQuestionsAsync(
            QuizMode.Training, subject.Id, topic.Id, null, null, null, 5, false, false);

        Assert.Equal(5, questions.Count);
    }

    [Fact]
    public async Task BuildQuizQuestionsAsync_ShufflesChoices_WhenRequested()
    {
        using var context = TestDbContextFactory.Create();
        var subject = TestDataBuilder.CreateSubject(context);
        var topic = TestDataBuilder.CreateTopic(context, subject.Id);
        TestDataBuilder.CreateQuestion(context, topic.Id, "Q1");

        var repo = new QuestionRepository(context);
        var srService = new SpacedRepetitionService(context, repo);
        var statsService = new StatisticsService(context);
        var quizService = new QuizService(context, repo, srService, statsService);

        var questions = await quizService.BuildQuizQuestionsAsync(
            QuizMode.Training, subject.Id, topic.Id, null, null, null, 1, false, shuffleChoices: true);

        Assert.Single(questions);
        Assert.Equal(4, questions[0].Choices.Count);
    }
}
