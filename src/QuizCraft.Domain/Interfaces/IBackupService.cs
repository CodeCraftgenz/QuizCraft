namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Serviço responsável por criar, restaurar e gerenciar backups do banco de dados.
/// </summary>
public interface IBackupService
{
    /// <summary>Cria um novo backup e retorna o caminho do arquivo gerado.</summary>
    Task<string> CreateBackupAsync();
    /// <summary>Restaura o banco de dados a partir de um arquivo de backup.</summary>
    /// <param name="backupFilePath">Caminho completo do arquivo de backup.</param>
    Task RestoreBackupAsync(string backupFilePath);
    /// <summary>Lista todos os backups disponíveis.</summary>
    Task<IReadOnlyList<BackupInfo>> GetBackupsAsync();
    /// <summary>Remove backups antigos, mantendo apenas a quantidade especificada.</summary>
    /// <param name="retentionCount">Quantidade de backups a manter.</param>
    Task DeleteOldBackupsAsync(int retentionCount);
    /// <summary>Retorna o diretório onde os backups são armazenados.</summary>
    Task<string> GetBackupDirectoryAsync();
}

/// <summary>
/// Informações sobre um arquivo de backup.
/// </summary>
/// <param name="FilePath">Caminho completo do arquivo.</param>
/// <param name="FileName">Nome do arquivo.</param>
/// <param name="CreatedAt">Data de criação do backup.</param>
/// <param name="SizeBytes">Tamanho do arquivo em bytes.</param>
public record BackupInfo(string FilePath, string FileName, DateTime CreatedAt, long SizeBytes);
