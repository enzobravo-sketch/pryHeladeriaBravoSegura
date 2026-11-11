using System.Text.Json;
using System.Text.Json.Serialization;

namespace pryHeladeriaBravoSegura.Data;

public sealed class ConfiguracionPersistencia
{
    public string CarpetaDatos { get; }
    public string RutaDatos { get; }
    public string RutaRespaldo { get; }
    public JsonSerializerOptions OpcionesJson { get; }

    public ConfiguracionPersistencia()
    {
        CarpetaDatos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GlupsStock");
        RutaDatos = Path.Combine(CarpetaDatos, "datos.json");
        RutaRespaldo = Path.Combine(CarpetaDatos, "datos.bak");
        OpcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
