using System.Text.Json;
using FluentValidation;

namespace MovieCatalog.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Any()
                ? ex.Errors.Select(e => (object)new { e.PropertyName, e.ErrorMessage }).ToList()
                : new List<object> { new { PropertyName = "Validation", ErrorMessage = ex.Message } };

            var response = new
            {
                Status = 400,
                Title = "Validation Error",
                Errors = errors,
                CorrelationId = context.Items["CorrelationId"]?.ToString()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Status = 404,
                Title = "Not Found",
                Detail = ex.Message,
                CorrelationId = context.Items["CorrelationId"]?.ToString()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                CorrelationId = context.Items["CorrelationId"]?.ToString()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
