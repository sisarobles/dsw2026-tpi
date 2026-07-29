using Dsw2026Tpi.Api.Configurations;
using Dsw2026Tpi.Api.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

namespace Dsw2026Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Inicializar con un logger simple antes de construir el host
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Iniciando aplicación Dsw2026Tpi.Api");

            var builder = WebApplication.CreateBuilder(args);

            //Configuraciones personalizadas
            builder.AddSerilogConfiguration();
            builder.Services.AddAppIdentity();
            builder.Services.AddAppAuthentication(builder.Configuration);
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddApplicationPersistence(builder.Configuration);
            builder.Services.AddAppCors(builder.Configuration);
            builder.Services.AddAppDependencies();
            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            app.UseSerilogRequestLogging(); //loguea que llegó un request (método, ruta, dureción)

            if (app.Environment.IsProduction())
            {
                app.UseHttpsRedirection(); //en producción, fuerza HTTPS
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); //permite el uso del swagger
                app.UseSwaggerUI(); //uso de la interfaz de swagger
            }

            app.UseAuthentication(); //lee el header de autorización (Bearer <token>) y arma el usuario con los claims que tengamos definidos
            app.UseAuthorization(); //chequear su el usuario tiene permiso para acceder a la ruta que solicita el request
            app.UseCors(); //verifica el origen del request
            app.UseMiddleware<ExceptionHandlingMiddleware>(); //envuelve todo lo siguiente en un try/catch para detectar errores especificamente en este caso

            app.MapControllers(); //permite que se ingrese a buscar qué controlador o método atiende la ruta solicitada
            app.MapHealthChecks("/health-check");

            Log.Information("Aplicación iniciada correctamente");

            await app.RunAsync();
        }
        catch (HostAbortedException)
        {
            Log.Information("El host fue abortado (normal durante migraciones de EF Core)");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "La aplicación falló al iniciar");
            throw;
        }
        finally
        {
            Log.Information("Cerrando aplicación");
            await Log.CloseAndFlushAsync();
        }
    }
}

