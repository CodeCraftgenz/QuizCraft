using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Tests.Helpers;

public static class TestDbContextFactory
{
    public static QuizCraftDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<QuizCraftDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new QuizCraftDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
