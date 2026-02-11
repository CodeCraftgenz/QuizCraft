using Microsoft.EntityFrameworkCore;

namespace QuizCraft.Infrastructure.Data;

/// <summary>
/// Inicializador do banco de dados. Responsavel por aplicar migrations
/// e fornecer os caminhos padrao de arquivos da aplicacao (banco, logs, backups, anexos).
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica todas as migrations pendentes no banco de dados.
    /// </summary>
    public static void Initialize(QuizCraftDbContext context)
    {
        context.Database.Migrate();
    }

    /// <summary>
    /// Retorna o caminho completo do arquivo SQLite. Cria a pasta se nao existir.
    /// </summary>
    public static string GetDatabasePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "QuizCraft");
        Directory.CreateDirectory(folder); // Garante que a pasta exista
        return Path.Combine(folder, "quizcraft.db");
    }

    /// <summary>
    /// Retorna o caminho da pasta de logs. Cria a pasta se nao existir.
    /// </summary>
    public static string GetLogsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "QuizCraft", "logs");
        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>
    /// Retorna o caminho da pasta de backups. Cria a pasta se nao existir.
    /// </summary>
    public static string GetBackupsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "QuizCraft", "backups");
        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>
    /// Retorna o caminho da pasta de anexos. Cria a pasta se nao existir.
    /// </summary>
    public static string GetAttachmentsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "QuizCraft", "attachments");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
