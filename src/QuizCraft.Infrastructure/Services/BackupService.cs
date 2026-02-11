using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;
using Serilog;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Servico de backup e restauracao do banco SQLite.
/// Cria arquivos ZIP contendo o banco de dados, WAL/SHM e anexos.
/// </summary>
public class BackupService : IBackupService
{
    private readonly QuizCraftDbContext _context;
    private readonly ILogger _logger;

    /// <summary>
    /// Inicializa o servico de backup com o contexto do banco.
    /// </summary>
    public BackupService(QuizCraftDbContext context)
    {
        _context = context;
        _logger = Log.ForContext<BackupService>();
    }

    /// <summary>
    /// Cria um backup completo em formato ZIP (banco SQLite + anexos).
    /// Fecha a conexao antes de copiar e reabre ao final.
    /// </summary>
    /// <returns>Caminho completo do arquivo ZIP criado.</returns>
    public async Task<string> CreateBackupAsync()
    {
        var backupDir = DatabaseInitializer.GetBackupsPath();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var zipPath = Path.Combine(backupDir, $"QuizCraft_Backup_{timestamp}.zip");

        _logger.Information("Creating backup at {Path}", zipPath);

        // Fecha todas as conexoes para garantir integridade do arquivo
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Closed)
            await connection.CloseAsync();

        try
        {
            var dbPath = DatabaseInitializer.GetDatabasePath();
            var attachmentsPath = DatabaseInitializer.GetAttachmentsPath();

            using var zipStream = new FileStream(zipPath, FileMode.Create);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            // Adiciona o arquivo principal do banco de dados
            if (File.Exists(dbPath))
            {
                archive.CreateEntryFromFile(dbPath, "quizcraft.db", CompressionLevel.Optimal);
            }

            // Inclui arquivos WAL e SHM do SQLite se existirem (journal mode)
            var walPath = dbPath + "-wal";
            var shmPath = dbPath + "-shm";
            if (File.Exists(walPath))
                archive.CreateEntryFromFile(walPath, "quizcraft.db-wal", CompressionLevel.Optimal);
            if (File.Exists(shmPath))
                archive.CreateEntryFromFile(shmPath, "quizcraft.db-shm", CompressionLevel.Optimal);

            // Inclui todos os anexos (imagens, arquivos) no ZIP
            if (Directory.Exists(attachmentsPath))
            {
                foreach (var file in Directory.GetFiles(attachmentsPath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(attachmentsPath, file);
                    archive.CreateEntryFromFile(file, Path.Combine("attachments", relativePath), CompressionLevel.Optimal);
                }
            }

            _logger.Information("Backup created successfully: {Path}", zipPath);
        }
        finally
        {
            await _context.Database.GetDbConnection().OpenAsync();
        }

        return zipPath;
    }

    /// <summary>
    /// Restaura o banco de dados a partir de um arquivo de backup ZIP.
    /// Cria um backup de seguranca do estado atual antes de restaurar.
    /// </summary>
    /// <param name="backupFilePath">Caminho do arquivo ZIP de backup.</param>
    public async Task RestoreBackupAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backup file not found.", backupFilePath);

        _logger.Information("Restoring backup from {Path}", backupFilePath);

        // Seguranca: cria backup do estado atual antes de sobrescrever
        await CreateBackupAsync();

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Closed)
            await connection.CloseAsync();

        try
        {
            var dbPath = DatabaseInitializer.GetDatabasePath();
            var attachmentsPath = DatabaseInitializer.GetAttachmentsPath();

            using var zipStream = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                string targetPath;

                if (entry.FullName == "quizcraft.db" || entry.FullName.StartsWith("quizcraft.db-"))
                {
                    targetPath = Path.Combine(Path.GetDirectoryName(dbPath)!, entry.FullName);
                }
                else if (entry.FullName.StartsWith("attachments/"))
                {
                    var relativePath = entry.FullName.Substring("attachments/".Length);
                    targetPath = Path.Combine(attachmentsPath, relativePath);
                }
                else
                {
                    continue;
                }

                var dir = Path.GetDirectoryName(targetPath);
                if (dir != null) Directory.CreateDirectory(dir);

                entry.ExtractToFile(targetPath, overwrite: true);
            }

            _logger.Information("Backup restored successfully from {Path}", backupFilePath);
        }
        finally
        {
            await _context.Database.GetDbConnection().OpenAsync();
        }
    }

    /// <summary>
    /// Lista todos os backups existentes, ordenados do mais recente ao mais antigo.
    /// </summary>
    public Task<IReadOnlyList<BackupInfo>> GetBackupsAsync()
    {
        var backupDir = DatabaseInitializer.GetBackupsPath();
        var files = Directory.GetFiles(backupDir, "QuizCraft_Backup_*.zip")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .Select(f => new BackupInfo(f.FullName, f.Name, f.CreationTime, f.Length))
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupInfo>>(files);
    }

    /// <summary>
    /// Remove backups antigos, mantendo apenas os N mais recentes conforme a politica de retencao.
    /// </summary>
    /// <param name="retentionCount">Quantidade de backups a manter.</param>
    public Task DeleteOldBackupsAsync(int retentionCount)
    {
        var backupDir = DatabaseInitializer.GetBackupsPath();
        var files = Directory.GetFiles(backupDir, "QuizCraft_Backup_*.zip")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .Skip(retentionCount)
            .ToList();

        foreach (var file in files)
        {
            _logger.Information("Deleting old backup: {Path}", file.FullName);
            file.Delete();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retorna o caminho do diretorio onde os backups sao armazenados.
    /// </summary>
    public Task<string> GetBackupDirectoryAsync()
    {
        return Task.FromResult(DatabaseInitializer.GetBackupsPath());
    }
}
