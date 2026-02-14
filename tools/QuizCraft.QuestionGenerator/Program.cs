// =============================================================================
// Program.cs - Orquestrador principal do gerador de questoes via OpenAI
//
// Uso:
//   1. Definir a chave: setx OPENAI_API_KEY "sk-..."  (PowerShell)
//   2. Rodar:           dotnet run --project tools/QuizCraft.QuestionGenerator
//
// Gera 1.000 questoes (10 materias x 4 lotes x 25 questoes) e importa
// diretamente no banco SQLite do QuizCraft via ImportExportService.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using QuizCraft.Application.Services;
using QuizCraft.Infrastructure.Data;
using QuizCraft.QuestionGenerator;

// =========================================================================
// Configuracoes
// =========================================================================

/// <summary>Numero maximo de tentativas por lote em caso de falha.</summary>
const int MaxTentativas = 3;

/// <summary>Delay minimo entre chamadas a API (ms).</summary>
const int DelayMinMs = 800;

/// <summary>Delay maximo entre chamadas a API (ms).</summary>
const int DelayMaxMs = 1500;

// =========================================================================
// Inicio do programa
// =========================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

// =========================================================================
// Modo de importacao manual: --import-manual
// Importa os JSONs da pasta manual-questions/ sem precisar da OpenAI
// =========================================================================

if (args.Contains("--import-manual"))
{
    Console.WriteLine("══════════════════════════════════════════════════════════");
    Console.WriteLine("  QuizCraft - Importacao Manual de Questoes              ");
    Console.WriteLine("══════════════════════════════════════════════════════════");
    Console.WriteLine();

    var dbPathManual = DatabaseInitializer.GetDatabasePath();
    Console.WriteLine($"[INFO] Banco: {dbPathManual}");

    var optionsManual = new DbContextOptionsBuilder<QuizCraftDbContext>();
    optionsManual.UseSqlite($"Data Source={dbPathManual}");

    if (!File.Exists(dbPathManual))
    {
        using var initCtx = new QuizCraftDbContext(optionsManual.Options);
        await initCtx.Database.EnsureCreatedAsync();
        Console.WriteLine("[OK] Banco criado.");
    }

    // Buscar JSONs na pasta manual-questions ao lado do executavel
    var manualDir = Path.Combine(AppContext.BaseDirectory, "manual-questions");
    // Fallback: pasta no diretorio do projeto (quando roda via dotnet run)
    if (!Directory.Exists(manualDir))
        manualDir = Path.Combine(Directory.GetCurrentDirectory(), "tools", "QuizCraft.QuestionGenerator", "manual-questions");
    if (!Directory.Exists(manualDir))
        manualDir = Path.Combine(Directory.GetCurrentDirectory(), "manual-questions");

    if (!Directory.Exists(manualDir))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERRO] Pasta manual-questions nao encontrada!");
        Console.ResetColor();
        return 1;
    }

    var jsonFiles = Directory.GetFiles(manualDir, "*.json");
    Console.WriteLine($"[INFO] Encontrados {jsonFiles.Length} arquivo(s) JSON em {manualDir}");
    Console.WriteLine();

    int totalImportadas = 0;
    foreach (var file in jsonFiles)
    {
        var fileName = Path.GetFileName(file);
        Console.Write($"  -> Importando {fileName}...");

        try
        {
            var json = await File.ReadAllTextAsync(file);
            using var ctx = new QuizCraftDbContext(optionsManual.Options);
            var importService = new ImportExportService(ctx);
            var qtd = await importService.ImportQuestionsJsonAsync(json);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" OK ({qtd} questoes)");
            Console.ResetColor();
            totalImportadas += qtd;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" ERRO: {ex.Message}");
            Console.ResetColor();
        }
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[OK] Total importadas: {totalImportadas} questoes");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Pressione qualquer tecla para sair...");
    Console.ReadKey();
    return 0;
}

// =========================================================================
// Modo normal: Geracao via OpenAI
// =========================================================================

Console.WriteLine("══════════════════════════════════════════════════════════");
Console.WriteLine("  QuizCraft - Gerador de Questoes via OpenAI            ");
Console.WriteLine("  10 materias x 4 lotes x ~25 questoes = ~1.000 total  ");
Console.WriteLine("══════════════════════════════════════════════════════════");
Console.WriteLine();

// =========================================================================
// 1. Obter chave da OpenAI
// =========================================================================

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[ERRO] Variavel de ambiente OPENAI_API_KEY nao definida!");
    Console.WriteLine();
    Console.WriteLine("  Defina com:");
    Console.WriteLine("    PowerShell: setx OPENAI_API_KEY \"sk-sua-chave\"");
    Console.WriteLine("    CMD:        set OPENAI_API_KEY=sk-sua-chave");
    Console.WriteLine();
    Console.WriteLine("  Depois feche e reabra o terminal.");
    Console.ResetColor();
    return 1;
}

Console.WriteLine($"[OK] Chave da OpenAI encontrada (termina em ...{apiKey[^4..]})");
Console.WriteLine();

// =========================================================================
// 2. Configurar banco de dados e servicos
// =========================================================================

Console.WriteLine("[INFO] Conectando ao banco de dados do QuizCraft...");

var dbPath = DatabaseInitializer.GetDatabasePath();
Console.WriteLine($"[INFO] Banco: {dbPath}");

var optionsBuilder = new DbContextOptionsBuilder<QuizCraftDbContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

// Verificar se o banco existe
if (!File.Exists(dbPath))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("[AVISO] Banco nao encontrado. Criando novo banco...");
    Console.ResetColor();
    using var initCtx = new QuizCraftDbContext(optionsBuilder.Options);
    await initCtx.Database.EnsureCreatedAsync();
    Console.WriteLine("[OK] Banco criado com sucesso.");
}

Console.WriteLine("[OK] Banco conectado.");
Console.WriteLine();

// =========================================================================
// 3. Carregar catalogo de materias e progresso anterior
// =========================================================================

var materias = SubjectCatalog.ObterMaterias();
var totalLotes = materias.Sum(m => m.Lotes.Length);
Console.WriteLine($"[INFO] Catalogo: {materias.Length} materias, {totalLotes} lotes, {totalLotes * 25} questoes planejadas");

// Controle de progresso - salva em %AppData%\QuizCraft\generator-progress.json
// Para resetar e gerar tudo do zero, passe --reset como argumento
var progressPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "QuizCraft", "generator-progress.json");
var progress = new ProgressTracker(progressPath);

// Verificar se o usuario quer resetar o progresso
if (args.Contains("--reset"))
{
    progress.Resetar();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("[INFO] Progresso resetado. Gerando tudo do zero.");
    Console.ResetColor();
}

if (progress.LotesConcluidos > 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[INFO] Progresso anterior: {progress.LotesConcluidos}/{totalLotes} lotes ja concluidos (serao pulados)");
    Console.WriteLine($"[INFO] Para resetar e gerar tudo do zero: dotnet run -- --reset");
    Console.ResetColor();
}

Console.WriteLine();

// =========================================================================
// 4. Processar cada materia e lote
// =========================================================================

using var openAi = new OpenAiClient(apiKey);
var random = new Random();

// Contadores globais
int lotesProcessados = 0;
int lotesComErro = 0;
int questoesImportadas = 0;
var inicio = DateTime.Now;

foreach (var materia in materias)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"{'=',-60}");
    Console.WriteLine($"  {materia.Nome}");
    Console.WriteLine($"{'=',-60}");
    Console.ResetColor();

    foreach (var lote in materia.Lotes)
    {
        lotesProcessados++;
        var progresso = $"[{lotesProcessados}/{totalLotes}]";
        var topicos = string.Join(", ", lote.Topicos);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  {progresso} Lote {lote.Numero} - {topicos}");
        Console.ResetColor();

        // Verificar se o lote ja foi concluido anteriormente
        if (progress.LoteJaConcluido(materia.Nome, lote.Numero))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    -> Ja concluido anteriormente. Pulando...");
            Console.ResetColor();
            continue;
        }

        // Tentativas com retry
        bool sucesso = false;
        for (int tentativa = 1; tentativa <= MaxTentativas; tentativa++)
        {
            try
            {
                if (tentativa > 1)
                {
                    Console.WriteLine($"    -> Tentativa {tentativa}/{MaxTentativas}...");
                }

                // Passo 1: Chamar OpenAI
                Console.Write("    -> Gerando questoes via OpenAI...");
                var jsonLote = await openAi.GerarLoteAsync(materia.Nome, lote);
                Console.WriteLine(" OK");

                // Passo 2: Validar JSON
                Console.Write("    -> Validando JSON...");
                var validacao = QuestionValidator.Validar(jsonLote, materia.Nome);

                if (!validacao.Valido)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" FALHA");
                    Console.WriteLine($"      Erro: {validacao.Erro}");
                    Console.ResetColor();

                    if (tentativa < MaxTentativas)
                    {
                        Console.WriteLine($"      Aguardando para nova tentativa...");
                        await Task.Delay(2000);
                        continue;
                    }

                    throw new Exception($"Validacao falhou apos {MaxTentativas} tentativas: {validacao.Erro}");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" OK ({validacao.TotalQuestoes} questoes)");
                Console.ResetColor();

                // Passo 3: Importar no banco via ImportExportService
                Console.Write("    -> Importando no banco...");

                // Cria um novo contexto para cada importacao (evita tracking issues)
                using (var ctx = new QuizCraftDbContext(optionsBuilder.Options))
                {
                    var importService = new ImportExportService(ctx);
                    var qtd = await importService.ImportQuestionsJsonAsync(jsonLote);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" OK ({qtd} importadas)");
                    Console.ResetColor();

                    questoesImportadas += qtd;
                }

                // Salvar progresso - marca lote como concluido no disco
                progress.MarcarLoteConcluido(materia.Nome, lote.Numero, validacao.TotalQuestoes);

                sucesso = true;
                break; // Sai do loop de tentativas
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" ERRO");
                Console.WriteLine($"      HTTP: {ex.Message}");
                Console.ResetColor();

                if (tentativa < MaxTentativas)
                {
                    var espera = tentativa * 3000; // Backoff progressivo
                    Console.WriteLine($"      Aguardando {espera / 1000}s para retry...");
                    await Task.Delay(espera);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" ERRO");
                Console.WriteLine($"      {ex.Message}");
                Console.ResetColor();

                if (tentativa < MaxTentativas)
                {
                    var espera = tentativa * 2000;
                    Console.WriteLine($"      Aguardando {espera / 1000}s para retry...");
                    await Task.Delay(espera);
                }
            }
        }

        if (!sucesso)
        {
            lotesComErro++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    X LOTE FALHOU apos {MaxTentativas} tentativas!");
            Console.ResetColor();
        }

        // Delay entre chamadas para nao estourar rate limit
        if (lotesProcessados < totalLotes)
        {
            var delay = random.Next(DelayMinMs, DelayMaxMs);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    Aguardando {delay}ms antes do proximo lote...");
            Console.ResetColor();
            await Task.Delay(delay);
        }
    }
}

// =========================================================================
// 5. Resumo final
// =========================================================================

var duracao = DateTime.Now - inicio;

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("══════════════════════════════════════════════════════════");
Console.WriteLine("                    RESUMO FINAL                         ");
Console.WriteLine("══════════════════════════════════════════════════════════");
Console.ResetColor();

Console.WriteLine($"  Lotes processados:  {lotesProcessados - lotesComErro}/{totalLotes} com sucesso");

if (lotesComErro > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  Lotes com erro:     {lotesComErro}");
    Console.ResetColor();
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"  Questoes importadas: {questoesImportadas}");
Console.ResetColor();

Console.WriteLine($"  Tempo total:         {duracao:hh\\:mm\\:ss}");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("══════════════════════════════════════════════════════════");
Console.ResetColor();

if (lotesComErro > 0)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
    Console.WriteLine($"[AVISO] {lotesComErro} lote(s) falharam. Rode novamente para tentar gerar os faltantes.");
    Console.ResetColor();
}

Console.WriteLine();
Console.WriteLine("Pressione qualquer tecla para sair...");
Console.ReadKey();

return lotesComErro > 0 ? 1 : 0;
