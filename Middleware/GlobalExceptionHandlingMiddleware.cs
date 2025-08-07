using System.Net;
using System.Text.Json;
using UserAuthAPI.DTOs;

namespace UserAuthAPI.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "An unhandled exception occurred");

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "An internal server error occurred",
            Errors = new List<string>()
        };

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                response.Message = "Invalid request parameters";
                response.Errors.Add(exception.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                response.Message = "Unauthorized access";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case KeyNotFoundException:
                response.Message = "Resource not found";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case InvalidOperationException:
                response.Message = "Invalid operation";
                response.Errors.Add(exception.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case TimeoutException:
                response.Message = "Request timeout";
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                break;

            default:
                response.Message = "An internal server error occurred";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                
                // Only include exception details in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    response.Errors.Add(exception.Message);
                    response.Errors.Add(exception.StackTrace ?? "No stack trace available");
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}