// =============================================================================
// QuestionValidator.cs - Validacao do JSON de questoes retornado pela OpenAI
// Verifica formato, campos obrigatorios, dificuldade e alternativas.
// =============================================================================

using System.Text.Json;

namespace QuizCraft.QuestionGenerator;

/// <summary>
/// Resultado da validacao de um lote de questoes.
/// </summary>
public record ValidacaoResultado(
    bool Valido,
    string? Erro,
    int TotalQuestoes
);

/// <summary>
/// Valida o JSON retornado pela OpenAI antes de importar.
/// Garante que o formato e compativel com o ImportExportService.
/// </summary>
public static class QuestionValidator
{
    /// <summary>Numero esperado de questoes por lote.</summary>
    private const int QuestoesEsperadas = 30;

    /// <summary>Numero esperado de alternativas por questao.</summary>
    private const int AlternativasEsperadas = 4;

    /// <summary>
    /// Valida o JSON de um lote de questoes.
    /// Retorna resultado com status e mensagem de erro (se houver).
    /// </summary>
    /// <param name="json">JSON contendo o array de questoes.</param>
    /// <param name="materiaEsperada">Nome da materia esperada.</param>
    /// <returns>Resultado da validacao.</returns>
    public static ValidacaoResultado Validar(string json, string materiaEsperada)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            return new ValidacaoResultado(false, $"JSON inválido: {ex.Message}", 0);
        }

        using (doc)
        {
            var root = doc.RootElement;

            // Verificar se e um array
            if (root.ValueKind != JsonValueKind.Array)
                return new ValidacaoResultado(false, "JSON não é um array.", 0);

            var total = root.GetArrayLength();

            // Verificar quantidade de questoes
            if (total < QuestoesEsperadas)
                return new ValidacaoResultado(false,
                    $"Lote incompleto: {total} questões (esperado {QuestoesEsperadas}).", total);

            // Validar cada questao individualmente
            int indice = 0;
            foreach (var questao in root.EnumerateArray())
            {
                indice++;
                var erro = ValidarQuestao(questao, indice, materiaEsperada);
                if (erro != null)
                    return new ValidacaoResultado(false, erro, total);
            }

            return new ValidacaoResultado(true, null, total);
        }
    }

    /// <summary>
    /// Valida uma questao individual.
    /// Verifica campos obrigatorios, dificuldade e alternativas.
    /// </summary>
    private static string? ValidarQuestao(JsonElement questao, int indice, string materiaEsperada)
    {
        // Campos obrigatorios de texto
        string[] camposTexto = ["Subject", "Topic", "Type", "Statement", "Explanation"];
        foreach (var campo in camposTexto)
        {
            if (!questao.TryGetProperty(campo, out var prop) ||
                prop.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(prop.GetString()))
            {
                return $"Questão {indice}: campo '{campo}' ausente ou vazio.";
            }
        }

        // Verificar se Type e MultipleChoice
        var type = questao.GetProperty("Type").GetString();
        if (type != "MultipleChoice")
            return $"Questão {indice}: Type deve ser 'MultipleChoice', mas é '{type}'.";

        // Verificar Difficulty (1 a 5)
        if (!questao.TryGetProperty("Difficulty", out var diff) ||
            diff.ValueKind != JsonValueKind.Number)
        {
            return $"Questão {indice}: campo 'Difficulty' ausente ou não numérico.";
        }

        var dificuldade = diff.GetInt32();
        if (dificuldade < 1 || dificuldade > 5)
            return $"Questão {indice}: Difficulty {dificuldade} fora do intervalo 1-5.";

        // Verificar Tags (array nao vazio)
        if (!questao.TryGetProperty("Tags", out var tags) ||
            tags.ValueKind != JsonValueKind.Array ||
            tags.GetArrayLength() == 0)
        {
            return $"Questão {indice}: campo 'Tags' ausente ou vazio.";
        }

        // Verificar Choices (array com 4 itens)
        if (!questao.TryGetProperty("Choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array)
        {
            return $"Questão {indice}: campo 'Choices' ausente.";
        }

        var totalChoices = choices.GetArrayLength();
        if (totalChoices != AlternativasEsperadas)
            return $"Questão {indice}: {totalChoices} alternativas (esperado {AlternativasEsperadas}).";

        // Verificar se cada Choice tem Text e IsCorrect, e exatamente 1 correta
        int corretas = 0;
        int choiceIdx = 0;
        foreach (var choice in choices.EnumerateArray())
        {
            choiceIdx++;
            if (!choice.TryGetProperty("Text", out var text) ||
                text.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(text.GetString()))
            {
                return $"Questão {indice}, alternativa {choiceIdx}: 'Text' ausente ou vazio.";
            }

            if (!choice.TryGetProperty("IsCorrect", out var isCorrect) ||
                isCorrect.ValueKind != JsonValueKind.True && isCorrect.ValueKind != JsonValueKind.False)
            {
                return $"Questão {indice}, alternativa {choiceIdx}: 'IsCorrect' ausente.";
            }

            if (isCorrect.GetBoolean())
                corretas++;
        }

        if (corretas != 1)
            return $"Questão {indice}: {corretas} alternativa(s) correta(s) (esperado exatamente 1).";

        return null; // Questao valida
    }
}
