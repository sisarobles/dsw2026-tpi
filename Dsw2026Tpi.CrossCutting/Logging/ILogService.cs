using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dsw2026Tpi.CrossCutting.Logging
{
    public interface ILogService
    {
        Task RegistrarAsync(
            string modulo,
            string accion,
            string detalle,
            LogNivel nivel = LogNivel.Info);

    }
    public enum LogNivel
    {
        Info,
        Warning,
        Error 
    }
}
