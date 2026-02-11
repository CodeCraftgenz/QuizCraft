using System.Management;
using Serilog;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Helper para identificacao de hardware.
/// Gera um fingerprint unico do dispositivo usando WMI (ProcessorId + MotherboardSerial).
/// </summary>
public static class HardwareHelper
{
    private static readonly ILogger Logger = Log.ForContext(typeof(HardwareHelper));

    /// <summary>
    /// Gera o identificador unico do hardware (fingerprint).
    /// Combina o ProcessorId e o SerialNumber da placa-mae, aplicando SHA-256.
    /// </summary>
    /// <returns>Hash SHA-256 do hardware em formato hexadecimal.</returns>
    public static string GetHardwareId()
    {
        try
        {
            var processorId = GetWmiValue("Win32_Processor", "ProcessorId");
            var motherboardSerial = GetWmiValue("Win32_BaseBoard", "SerialNumber");
            var combined = $"{processorId}|{motherboardSerial}";
            return CryptoHelper.Sha256(combined);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Erro ao obter hardware ID via WMI");
            // Fallback: usa nome da maquina + usuario como identificador
            var fallback = $"{Environment.MachineName}|{Environment.UserName}";
            return CryptoHelper.Sha256(fallback);
        }
    }

    /// <summary>
    /// Consulta um valor especifico via WMI (Windows Management Instrumentation).
    /// </summary>
    /// <param name="wmiClass">Classe WMI (ex: Win32_Processor).</param>
    /// <param name="propertyName">Propriedade desejada (ex: ProcessorId).</param>
    /// <returns>Valor da propriedade ou "Unknown" se nao encontrado.</returns>
    private static string GetWmiValue(string wmiClass, string propertyName)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}");
            foreach (var obj in searcher.Get())
            {
                var value = obj[propertyName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Erro ao consultar WMI {Class}.{Property}", wmiClass, propertyName);
        }
        return "Unknown";
    }
}
