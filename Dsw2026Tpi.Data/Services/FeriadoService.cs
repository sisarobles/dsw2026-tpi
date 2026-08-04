using Dsw2026Tpi.Data.Options;
using Dsw2026Tpi.Domain.Interfaces;
using System.Text.Json;

namespace Dsw2026Tpi.Data.Services
{
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

}
