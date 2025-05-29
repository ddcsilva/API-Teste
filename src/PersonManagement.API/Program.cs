using PersonManagement.API.Extensions;
using PersonManagement.Logging.Extensions;

namespace PersonManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== CONFIGURAÇÃO LOGGING =====
            builder.Services.AddAppLogging(builder.Configuration, builder.Environment);

            // ===== CONFIGURAÇÃO DE SERVIÇOS =====
            builder.Services
                .AddApiConfiguration()
                .AddSwaggerDocumentation()
                .AddCorsPolicy()
                .AddApplicationServices()
                .AddInfrastructureServices(builder.Configuration)
                .AddAdvancedHealthChecks(builder.Configuration);

            var app = builder.Build();

            // ===== CONFIGURAÇÃO DO PIPELINE =====
            app.UseAppLogging()
               .ConfigurePipeline()
               .ConfigureAdvancedHealthChecks()
               .SeedDatabase();

            // Configurar shutdown graceful do logging
            app.ConfigureLoggingShutdown();

            app.Run();
        }
        catch (Exception ex)
        {
            // Log de erro fatal usando console como fallback
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Program");
            logger.LogCritical(ex, "💥 Aplicação falhou ao inicializar");
        }
    }
}
