using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;
using PersonManagement.Application.Mappings;
using PersonManagement.Domain.Interfaces;
using PersonManagement.Infrastructure.Data;
using System.Reflection;

namespace PersonManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CriarPessoaCommand).Assembly));

        // AutoMapper
        services.AddAutoMapper(typeof(PessoaMappingProfile));

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(CriarPessoaCommandValidator).Assembly);

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework com SQLite
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var databaseType = configuration.GetValue<string>("DatabaseType", "Sqlite");

            switch (databaseType?.ToLower() ?? "sqlite")
            {
                case "sqlite":
                    var connectionString = configuration.GetConnectionString("SqliteConnection")
                        ?? "Data Source=PersonManagement.db";
                    options.UseSqlite(connectionString);
                    break;

                case "inmemory":
                    options.UseInMemoryDatabase("PersonManagementDb");
                    break;

                case "sqlserver":
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    break;

                default:
                    // Padrão SQLite
                    options.UseSqlite("Data Source=PersonManagement.db");
                    break;
            }

            // Configurações para desenvolvimento
            if (configuration.GetValue<bool>("Database:EnableSensitiveDataLogging", false))
            {
                options.EnableSensitiveDataLogging();
            }

            if (configuration.GetValue<bool>("Database:EnableDetailedErrors", false))
            {
                options.EnableDetailedErrors();
            }
        });

        // Dependency Injection
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Gerenciamento de Pessoas API",
                Version = "v1.0",
                Description = "API para gerenciamento de pessoas seguindo Clean Architecture e CQRS",
                Contact = new()
                {
                    Name = "Sua Empresa",
                    Email = "contato@suaempresa.com",
                    Url = new Uri("https://suaempresa.com")
                },
                License = new()
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            // Política mais restritiva para produção
            options.AddPolicy("Production", policy =>
            {
                policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Configurações globais dos controllers
            options.SuppressAsyncSuffixInActionNames = false;
        });

        // Configuração do JSON
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null; // Manter nomes das propriedades
        });

        return services;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthCheckBuilder = services.AddHealthChecks();

        // Health Check do banco de dados
        var databaseType = configuration.GetValue<string>("DatabaseType", "Sqlite");

        switch (databaseType?.ToLower())
        {
            case "sqlite":
                healthCheckBuilder.AddSqlite(
                    configuration.GetConnectionString("SqliteConnection") ?? "Data Source=PersonManagement.db",
                    name: "database");
                break;

            case "sqlserver":
                healthCheckBuilder.AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
                    name: "database");
                break;
        }

        // Health Check customizado para a aplicação
        healthCheckBuilder.AddCheck<ApplicationHealthCheck>("application");

        return services;
    }
}