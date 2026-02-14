using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizCraft.Domain.Models;
using Serilog;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Cliente HTTP para comunicacao com a API de licenciamento.
/// Envia requisicoes de ativacao e verificacao de licenca ao servidor.
/// </summary>
public class LicenseApiClient
{
    private static readonly ILogger Logger = Log.ForContext<LicenseApiClient>();

    /// <summary>URL base da API de licenciamento.</summary>
    private const string BaseUrl = "https://codecraftgenz-monorepo.onrender.com/api";

    /// <summary>
    /// ID do aplicativo QuizCraft no backend.
    /// Deve corresponder ao registro na tabela de apps do servidor.
    /// </summary>
    private const int AppId = 10;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa o cliente com timeout padrao de 30 segundos.
    /// </summary>
    public LicenseApiClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Ativa o dispositivo atual na API, vinculando o email ao hardware.
    /// Endpoint: POST /api/public/license/activate-device
    /// </summary>
    /// <param name="email">Email do usuario.</param>
    /// <param name="hardwareId">Identificador unico do hardware.</param>
    /// <returns>Resultado da ativacao.</returns>
    public async Task<DeviceActivationResult> ActivateDeviceAsync(string email, string hardwareId)
    {
        try
        {
            var payload = new
            {
                app_id = AppId,
                email = email.Trim().ToLowerInvariant(),
                hardware_id = hardwareId
            };

            Logger.Information("Ativando dispositivo para {Email}...", email);

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/public/license/activate-device", payload, JsonOptions);

            var json = await response.Content.ReadAsStringAsync();
            Logger.Debug("Resposta da ativacao: {StatusCode} - {Body}", response.StatusCode, json);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ActivateDeviceResponse>(json, JsonOptions);
                return new DeviceActivationResult
                {
                    Success = result?.Success ?? false,
                    Message = result?.Message ?? "Resposta inesperada do servidor",
                    LicenseKey = result?.LicenseKey,
                    AppName = result?.AppName
                };
            }

            // Tenta extrair mensagem de erro do corpo da resposta
            var errorMsg = TryExtractErrorMessage(json) ?? $"Erro do servidor ({response.StatusCode})";
            return new DeviceActivationResult { Success = false, Message = errorMsg };
        }
        catch (TaskCanceledException)
        {
            Logger.Warning("Timeout ao ativar dispositivo");
            return new DeviceActivationResult
            {
                Success = false,
                Message = "Servidor nao respondeu. Verifique sua conexao e tente novamente."
            };
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex, "Erro de conexao ao ativar dispositivo");
            return new DeviceActivationResult
            {
                Success = false,
                Message = "Sem conexao com o servidor. Verifique sua internet."
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Erro inesperado ao ativar dispositivo");
            return new DeviceActivationResult
            {
                Success = false,
                Message = $"Erro inesperado: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verifica se a licenca do dispositivo e valida na API.
    /// Endpoint: POST /api/verify-license
    /// </summary>
    /// <param name="email">Email do usuario.</param>
    /// <param name="hardwareId">Identificador unico do hardware.</param>
    /// <returns>Resultado da verificacao.</returns>
    public async Task<LicenseValidationResult> VerifyLicenseAsync(string email, string hardwareId)
    {
        try
        {
            var payload = new
            {
                app_id = AppId,
                email = email.Trim().ToLowerInvariant(),
                hardware_id = hardwareId
            };

            Logger.Debug("Verificando licenca para {Email}...", email);

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/verify-license", payload, JsonOptions);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<VerifyLicenseResponse>(json, JsonOptions);
                return new LicenseValidationResult
                {
                    Valid = result?.Valid ?? false,
                    Message = result?.Message ?? "Resposta inesperada",
                    LicenseKey = result?.LicenseKey,
                    ActivatedAt = result?.ActivatedAt
                };
            }

            return new LicenseValidationResult
            {
                Valid = false,
                Message = TryExtractErrorMessage(json) ?? "Licenca nao encontrada"
            };
        }
        catch (TaskCanceledException)
        {
            return new LicenseValidationResult
            {
                Valid = false,
                Message = "Timeout na verificacao. Usando licenca offline."
            };
        }
        catch (HttpRequestException)
        {
            return new LicenseValidationResult
            {
                Valid = false,
                Message = "Sem conexao. Usando licenca offline."
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Erro ao verificar licenca");
            return new LicenseValidationResult
            {
                Valid = false,
                Message = $"Erro: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Tenta extrair a mensagem de erro de um JSON de resposta.
    /// </summary>
    private static string? TryExtractErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            // Formato: { "error": { "message": "..." } }
            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                if (err.ValueKind == JsonValueKind.Object &&
                    err.TryGetProperty("message", out var errMsg))
                    return errMsg.GetString();
                if (err.ValueKind == JsonValueKind.String)
                    return err.GetString();
            }
            // Formato: { "message": "..." }
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString();
        }
        catch { }
        return null;
    }

    // Classes internas para deserializacao das respostas da API

    private class ActivateDeviceResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("license_key")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("app_name")]
        public string? AppName { get; set; }
    }

    private class VerifyLicenseResponse
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("license_key")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("activated_at")]
        public DateTime? ActivatedAt { get; set; }
    }
}
