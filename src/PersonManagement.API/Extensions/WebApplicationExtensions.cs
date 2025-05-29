using Microsoft.EntityFrameworkCore;
using PersonManagement.Infrastructure.Data;

namespace PersonManagement.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.ConfigureDevelopmentPipeline();
        }
        else
        {
            app.ConfigureProductionPipeline();
        }

        app.ConfigureCommonPipeline();

        return app;
    }

    private static WebApplication ConfigureDevelopmentPipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gerenciamento de Pessoas API v1.0");
            c.RoutePrefix = string.Empty; // Swagger na raiz
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
        });

        // Usar CORS mais permissivo em desenvolvimento
        app.UseCors("AllowAll");

        return app;
    }

    private static WebApplication ConfigureProductionPipeline(this WebApplication app)
    {
        // Configurações específicas para produção
        app.UseHsts();

        // Usar CORS mais restritivo em produção
        app.UseCors("Production");

        return app;
    }

    private static WebApplication ConfigureCommonPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication(); // Se implementar autenticação no futuro
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static WebApplication SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Verificar se é InMemory database (usado nos testes)
            var isInMemory = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

            if (isInMemory)
            {
                // Para InMemory, apenas garantir que o banco existe
                context.Database.EnsureCreated();
                logger.LogInformation("✅ InMemory database configurado para testes");
            }
            else
            {
                // Para bancos relacionais, aplicar migrations
                if (context.Database.GetPendingMigrations().Any())
                {
                    logger.LogInformation("🔄 Aplicando migrations pendentes...");
                    context.Database.Migrate();
                    logger.LogInformation("✅ Migrations aplicadas com sucesso");
                }
            }

            // Seed de dados apenas em desenvolvimento (e não em testes)
            if (app.Environment.IsDevelopment() && !app.Environment.EnvironmentName.Contains("Testing"))
            {
                SeedDevelopmentData(context, logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro ao configurar banco de dados");
            // Em ambiente de teste, não falhar se der erro no seed
            if (!app.Environment.EnvironmentName.Contains("Testing"))
            {
                throw;
            }
        }

        return app;
    }

    private static void SeedDevelopmentData(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Pessoas.Any())
        {
            logger.LogInformation("🌱 Inserindo dados de exemplo...");

            var pessoas = new[]
            {
                new PersonManagement.Domain.Entities.Pessoa("João", "Silva", "joao.silva@email.com", new DateTime(1990, 5, 15), "12345678901"),
                new PersonManagement.Domain.Entities.Pessoa("Maria", "Santos", "maria.santos@email.com", new DateTime(1985, 8, 22), "98765432109"),
                new PersonManagement.Domain.Entities.Pessoa("Pedro", "Oliveira", "pedro.oliveira@email.com", new DateTime(1992, 12, 3), "11122233344"),
                new PersonManagement.Domain.Entities.Pessoa("Ana", "Costa", "ana.costa@email.com", new DateTime(1988, 3, 10), "55566677788"),
                new PersonManagement.Domain.Entities.Pessoa("Carlos", "Lima", "carlos.lima@email.com", new DateTime(1995, 11, 25), "99988877766")
            };

            context.Pessoas.AddRange(pessoas);
            context.SaveChanges();

            logger.LogInformation("✅ {Count} pessoas inseridas como dados de exemplo", pessoas.Length);
        }
        else
        {
            logger.LogInformation("📊 Banco de dados já possui {Count} pessoas", context.Pessoas.Count());
        }
    }
}