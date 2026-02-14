// =============================================================================
// ProgressTracker.cs - Controle de progresso para retomada apos interrupcao
//
// Salva em um arquivo JSON quais lotes ja foram processados com sucesso.
// Ao reiniciar, os lotes concluidos sao automaticamente pulados,
// evitando duplicatas e desperdicio de tokens.
//
// Arquivo de progresso: %AppData%\QuizCraft\generator-progress.json
// Para resetar e gerar tudo do zero, basta deletar esse arquivo.
// =============================================================================

using System.Text.Json;

namespace QuizCraft.QuestionGenerator;

/// <summary>
/// Controla quais lotes ja foram processados com sucesso.
/// Persiste o estado em arquivo JSON para retomada apos interrupcao.
/// </summary>
public class ProgressTracker
{
    private readonly string _filePath;
    private readonly HashSet<string> _lotesCompletos;

    /// <summary>Quantidade de lotes ja concluidos.</summary>
    public int LotesConcluidos => _lotesCompletos.Count;

    /// <summary>
    /// Inicializa o tracker carregando o progresso salvo (se existir).
    /// </summary>
    /// <param name="filePath">Caminho do arquivo de progresso.</param>
    public ProgressTracker(string filePath)
    {
        _filePath = filePath;
        _lotesCompletos = CarregarProgresso();
    }

    /// <summary>
    /// Gera uma chave unica para identificar um lote (Materia + Numero do lote).
    /// </summary>
    public static string GerarChave(string materia, int loteNumero) =>
        $"{materia}::Lote{loteNumero}";

    /// <summary>
    /// Verifica se um lote ja foi processado com sucesso.
    /// </summary>
    public bool LoteJaConcluido(string materia, int loteNumero)
    {
        var chave = GerarChave(materia, loteNumero);
        return _lotesCompletos.Contains(chave);
    }

    /// <summary>
    /// Marca um lote como concluido e salva o progresso no disco.
    /// </summary>
    public void MarcarLoteConcluido(string materia, int loteNumero, int questoesImportadas)
    {
        var chave = GerarChave(materia, loteNumero);
        _lotesCompletos.Add(chave);
        SalvarProgresso();
    }

    /// <summary>
    /// Remove o arquivo de progresso para comecar tudo do zero.
    /// </summary>
    public void Resetar()
    {
        _lotesCompletos.Clear();
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    /// <summary>
    /// Carrega o progresso salvo do arquivo JSON.
    /// </summary>
    private HashSet<string> CarregarProgresso()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<ProgressData>(json);
            return data?.LotesCompletos?.ToHashSet() ?? [];
        }
        catch
        {
            // Se o arquivo estiver corrompido, comecar do zero
            return [];
        }
    }

    /// <summary>
    /// Salva o progresso atual no arquivo JSON.
    /// </summary>
    private void SalvarProgresso()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var data = new ProgressData
        {
            UltimaAtualizacao = DateTime.Now,
            LotesCompletos = _lotesCompletos.Order().ToList()
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Estrutura do arquivo de progresso.
    /// </summary>
    private class ProgressData
    {
        public DateTime UltimaAtualizacao { get; set; }
        public List<string> LotesCompletos { get; set; } = [];
    }
}
