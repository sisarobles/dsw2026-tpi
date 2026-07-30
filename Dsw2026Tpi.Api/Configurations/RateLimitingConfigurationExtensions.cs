using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations
{
    public  static class RateLimitingConfigurationExtensions 
    {
       
            public static IServiceCollection AddAppRateLimiting(
                this IServiceCollection services,
                IConfiguration configuration)
            {
                var adminLimit = configuration.GetValue<int>("RateLimiting:AdminLogin:PermitLimit");
                var adminWindow = configuration.GetValue<int>("RateLimiting:AdminLogin:Window");
                var patientLimit = configuration.GetValue<int>("RateLimiting:PatientLogin:PermitLimit");
                var patientWindow = configuration.GetValue<int>("RateLimiting:PatientLogin:Window");
                var appointmentLimit = configuration.GetValue<int>("RateLimiting:Appointments:PermitLimit");
                var appointmentWindow = configuration.GetValue<int>("RateLimiting:Appointments:Window");
                var generalLimit = configuration.GetValue<int>("RateLimiting:General:PermitLimit");
                var generalWindow = configuration.GetValue<int>("RateLimiting:General:Window");

                services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    options.OnRejected = async (context, token) =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(
                            "Rate limit excedido: {Path} desde {IP}",
                            context.HttpContext.Request.Path,
                            context.HttpContext.Connection.RemoteIpAddress);

                        context.HttpContext.Response.ContentType = "application/json";
                        var errorResponse = new ErrorResponse(
                            nameof(ErrorCodes.TOO_MANY_REQUESTS),
                            ErrorCodes.TOO_MANY_REQUESTS);
                        await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, token);
                    };

                    // Admin login — 5 req/min por IP
                    options.AddFixedWindowLimiter("AdminAuthPolicy", opt =>
                    {
                        opt.PermitLimit = adminLimit;
                        opt.Window = TimeSpan.FromMinutes(adminWindow);
                        opt.QueueLimit = 0;
                    });

                    // Patient login — 10 req/min por IP
                    options.AddFixedWindowLimiter("PatientAuthPolicy", opt =>
                    {
                        opt.PermitLimit = patientLimit;
                        opt.Window = TimeSpan.FromMinutes(patientWindow);
                        opt.QueueLimit = 0;
                    });

                    // Appointments — 5 req/min por usuario autenticado
                    options.AddPolicy<string>("AppointmentPolicy", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User.Identity?.Name
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = appointmentLimit,
            Window = TimeSpan.FromMinutes(appointmentWindow),
            QueueLimit = 0
        }));

                    // General — 100 req/min por usuario o IP
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.User.Identity?.Name
                                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = generalLimit,
                                Window = TimeSpan.FromMinutes(generalWindow),
                                QueueLimit = 0
                            }));
                });

                return services;
            }
        }

    }

