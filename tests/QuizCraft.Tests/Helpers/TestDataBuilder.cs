using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Tests.Helpers;

public static class TestDataBuilder
{
    public static Subject CreateSubject(QuizCraftDbContext context, string name = "Matemática")
    {
        var subject = new Subject
        {
            Name = name,
            Description = $"Matéria: {name}",
            Color = "#2196F3",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Subjects.Add(subject);
        context.SaveChanges();
        return subject;
    }

    public static Topic CreateTopic(QuizCraftDbContext context, int subjectId, string name = "Álgebra")
    {
        var topic = new Topic
        {
            Name = name,
            SubjectId = subjectId,
            CreatedAt = DateTime.UtcNow
        };
        context.Topics.Add(topic);
        context.SaveChanges();
        return topic;
    }

    public static Question CreateQuestion(
        QuizCraftDbContext context,
        int topicId,
        string statement = "Qual é 2 + 2?",
        int difficulty = 1,
        QuestionType type = QuestionType.MultipleChoice,
        bool withChoices = true)
    {
        var question = new Question
        {
            TopicId = topicId,
            Type = type,
            Statement = statement,
            Difficulty = difficulty,
            Explanation = "Explicação de teste.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Questions.Add(question);
        context.SaveChanges();

        if (withChoices && type == QuestionType.MultipleChoice)
        {
            context.Choices.AddRange(
                new Choice { QuestionId = question.Id, Text = "3", IsCorrect = false, Order = 0 },
                new Choice { QuestionId = question.Id, Text = "4", IsCorrect = true, Order = 1 },
                new Choice { QuestionId = question.Id, Text = "5", IsCorrect = false, Order = 2 },
                new Choice { QuestionId = question.Id, Text = "6", IsCorrect = false, Order = 3 }
            );
            context.SaveChanges();
        }

        return question;
    }

    public static QuizSession CreateSession(
        QuizCraftDbContext context,
        QuizMode mode = QuizMode.Training,
        SessionStatus status = SessionStatus.Completed,
        int totalQuestions = 5,
        int correctCount = 3,
        int durationSeconds = 120)
    {
        var session = new QuizSession
        {
            Mode = mode,
            Status = status,
            TotalQuestions = totalQuestions,
            CorrectCount = correctCount,
            DurationSeconds = durationSeconds,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            EndedAt = status == SessionStatus.Completed ? DateTime.UtcNow : null
        };
        context.QuizSessions.Add(session);
        context.SaveChanges();
        return session;
    }

    public static QuizSessionItem CreateSessionItem(
        QuizCraftDbContext context,
        int sessionId,
        int questionId,
        bool isCorrect = true,
        int timeSeconds = 10,
        int order = 0)
    {
        var item = new QuizSessionItem
        {
            SessionId = sessionId,
            QuestionId = questionId,
            IsCorrect = isCorrect,
            TimeSeconds = timeSeconds,
            Order = order,
            SelectedAnswerJson = isCorrect ? "\"4\"" : "\"3\""
        };
        context.QuizSessionItems.Add(item);
        context.SaveChanges();
        return item;
    }
}
