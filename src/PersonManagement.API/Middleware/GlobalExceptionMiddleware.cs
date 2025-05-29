using PersonManagement.Domain.Exceptions;
using PersonManagement.Logging.Abstractions;
using System.Net;
using System.Text.Json;

namespace PersonManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppLogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, IAppLogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            BusinessValidationException businessEx => (HttpStatusCode.BadRequest, businessEx.Message),
            EntityNotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message),
            DuplicateEntityException duplicateEx => (HttpStatusCode.Conflict, duplicateEx.Message),
            ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro interno no servidor.")
        };

        context.Response.StatusCode = (int)statusCode;

        // Log da exceção
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "❌ Erro interno não tratado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("⚠️ Exceção de negócio: {ExceptionType} - {Message}",
                exception.GetType().Name, exception.Message);
        }

        var response = new
        {
            erro = message,
            tipo = exception.GetType().Name,
            timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}