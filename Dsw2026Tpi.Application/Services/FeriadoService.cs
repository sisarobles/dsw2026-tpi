using Dsw2026Tpi.Data.Options;
using System.Text.Json;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

/// <summary>
/// Lee feriados.json una sola vez (cacheado en memoria) y responde
/// consultas de "¿esta fecha es feriado?" sin volver a tocar disco.
/// Registrar como Singleton en DI, ya que el archivo no cambia en runtime.
/// </summary>
public class FeriadoService : IFeriadoService
{
    private readonly Dictionary<DateOnly, FeriadoInfo> _feriados;
    public FeriadoService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Sources", "feriados.json");

        if (!File.Exists(path))
        {
            _feriados = new Dictionary<DateOnly, FeriadoInfo>();
            return;
        }

        var json = File.ReadAllText(path);
        var raw = JsonSerializer.Deserialize<List<FeriadoJson>>(json, JsonOptions.JsonSerializerOptions)
                  ?? new List<FeriadoJson>();

        _feriados = raw.ToDictionary(
            f => DateOnly.Parse(f.Fecha),
            f => new FeriadoInfo(DateOnly.Parse(f.Fecha), f.Nombre, f.Tipo));
    }

    public bool EsFeriado(DateOnly fecha) => _feriados.ContainsKey(fecha);

    public FeriadoInfo? GetFeriado(DateOnly fecha) =>
        _feriados.TryGetValue(fecha, out var info) ? info : null;

    private record FeriadoJson(string Fecha, string Nombre, string Tipo);
}
