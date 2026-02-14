// =============================================================================
// OpenAiClient.cs - Cliente para a API da OpenAI (Responses API + Structured Outputs)
// Responsavel por montar o prompt, enviar para a API e retornar o JSON bruto.
// =============================================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace QuizCraft.QuestionGenerator;

/// <summary>
/// Cliente HTTP para a API da OpenAI.
/// Usa a Responses API com response_format=json_schema para garantir JSON estrito.
/// </summary>
public class OpenAiClient : IDisposable
{
    // =========================================================================
    // Configuracoes - facil de trocar o modelo aqui
    // =========================================================================

    /// <summary>Modelo da OpenAI a ser utilizado. Troque aqui se necessario.</summary>
    private const string Modelo = "gpt-4o-mini";

    /// <summary>Temperatura para geracao (0 = deterministic, 1 = creative).</summary>
    private const double Temperatura = 0.7;

    /// <summary>Maximo de tokens de saida para comportar ~50 questoes.</summary>
    private const int MaxOutputTokens = 16384;

    /// <summary>URL da Responses API da OpenAI.</summary>
    private const string ApiUrl = "https://api.openai.com/v1/responses";

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa o cliente com a chave da OpenAI.
    /// </summary>
    /// <param name="apiKey">Chave da API OpenAI.</param>
    public OpenAiClient(string apiKey)
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// <summary>
    /// Gera um lote de questoes chamando a API da OpenAI.
    /// Retorna o JSON bruto (array de questoes).
    /// </summary>
    /// <param name="materia">Nome da materia.</param>
    /// <param name="lote">Dados do lote (numero, nivel, topicos).</param>
    /// <returns>JSON bruto retornado pela API.</returns>
    public async Task<string> GerarLoteAsync(string materia, Lote lote)
    {
        // Monta o prompt de instrucao do sistema
        var systemPrompt = MontarPromptSistema(materia, lote);

        // Monta o corpo da requisicao com JSON Schema (Structured Outputs)
        var requestBody = MontarCorpoRequisicao(systemPrompt);

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        // Envia para a API
        var response = await _httpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content });

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API retornou {response.StatusCode}: {responseJson}");
        }

        // Extrai o conteudo de texto da resposta
        return ExtrairConteudo(responseJson);
    }

    /// <summary>
    /// Monta o prompt de instrucao do sistema para gerar o lote.
    /// </summary>
    private static string MontarPromptSistema(string materia, Lote lote)
    {
        var topicos = string.Join(", ", lote.Topicos);
        return $"""
            Você é um gerador de questões educacionais para estudantes brasileiros.

            TAREFA: Gerar EXATAMENTE 50 questões originais de múltipla escolha.

            MATÉRIA: {materia}
            NÍVEL: {lote.Nivel} (EF = Ensino Fundamental, EM = Ensino Médio)
            TÓPICOS: {topicos}
            LOTE: {lote.Numero}

            REGRAS OBRIGATÓRIAS:
            1. Todas as questões devem ser em Português do Brasil (PT-BR).
            2. Cada questão deve ter EXATAMENTE 4 alternativas.
            3. EXATAMENTE 1 alternativa deve ser correta (IsCorrect=true), as outras 3 false.
            4. O campo "Type" deve ser sempre "MultipleChoice".
            5. O campo "Subject" deve ser sempre "{materia}".
            6. Distribua as questões entre os tópicos: {topicos}.
            7. O campo "Tags" deve conter: ["{lote.Nivel}", "{materia}", "Lote{lote.Numero}", "<nome_do_topico>"].
            8. NÃO copie questões reais de provas. Crie questões ORIGINAIS.
            9. Inclua uma explicação curta e didática no campo "Explanation".

            DISTRIBUIÇÃO DE DIFICULDADE (total 50):
            - 10 questões com Difficulty=1 (muito fácil)
            - 10 questões com Difficulty=2 (fácil)
            - 20 questões com Difficulty=3 (média)
            - 5 questões com Difficulty=4 (difícil)
            - 5 questões com Difficulty=5 (muito difícil)

            FORMATO: Retorne um array JSON com exatamente 50 objetos.
            """;
    }

    /// <summary>
    /// Monta o corpo da requisicao para a Responses API com JSON Schema.
    /// Usa Structured Outputs para garantir formato estrito.
    /// </summary>
    private static object MontarCorpoRequisicao(string systemPrompt)
    {
        return new
        {
            model = Modelo,
            temperature = Temperatura,
            max_output_tokens = MaxOutputTokens,
            input = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = "Gere as 50 questões agora." }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "question_batch",
                    strict = true,
                    schema = ObterJsonSchema()
                }
            }
        };
    }

    /// <summary>
    /// Retorna o JSON Schema que define o formato das questoes.
    /// Usado pelo Structured Outputs para garantir aderencia ao formato.
    /// </summary>
    private static object ObterJsonSchema()
    {
        return new
        {
            type = "object",
            properties = new
            {
                questions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            Subject = new { type = "string" },
                            Topic = new { type = "string" },
                            Type = new { type = "string" },
                            Statement = new { type = "string" },
                            Explanation = new { type = "string" },
                            Difficulty = new { type = "integer" },
                            Tags = new { type = "array", items = new { type = "string" } },
                            Choices = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        Text = new { type = "string" },
                                        IsCorrect = new { type = "boolean" }
                                    },
                                    required = new[] { "Text", "IsCorrect" },
                                    additionalProperties = false
                                }
                            }
                        },
                        required = new[] { "Subject", "Topic", "Type", "Statement", "Explanation", "Difficulty", "Tags", "Choices" },
                        additionalProperties = false
                    }
                }
            },
            required = new[] { "questions" },
            additionalProperties = false
        };
    }

    /// <summary>
    /// Extrai o conteudo de texto da resposta da Responses API.
    /// A resposta vem em output[].content[].text
    /// </summary>
    private static string ExtrairConteudo(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        // Formato da Responses API: output[0].content[0].text
        if (root.TryGetProperty("output", out var output))
        {
            foreach (var item in output.EnumerateArray())
            {
                if (item.TryGetProperty("content", out var contentArray))
                {
                    foreach (var c in contentArray.EnumerateArray())
                    {
                        if (c.TryGetProperty("text", out var text))
                        {
                            var jsonText = text.GetString()
                                ?? throw new Exception("Resposta vazia da API.");

                            // O Structured Outputs retorna { "questions": [...] }
                            // Precisamos extrair o array interno
                            using var innerDoc = JsonDocument.Parse(jsonText);
                            if (innerDoc.RootElement.TryGetProperty("questions", out var questionsArray))
                            {
                                return questionsArray.GetRawText();
                            }

                            // Fallback: se ja e um array direto
                            return jsonText;
                        }
                    }
                }
            }
        }

        // Fallback para Chat Completions API (caso mude)
        if (root.TryGetProperty("choices", out var choices))
        {
            var message = choices[0].GetProperty("message");
            var content = message.GetProperty("content").GetString()
                ?? throw new Exception("Resposta vazia da API.");
            return content;
        }

        throw new Exception($"Formato de resposta inesperado da API: {responseJson[..Math.Min(500, responseJson.Length)]}");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
