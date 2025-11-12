using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["ApiKey:Key"]!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip the middleware for Swagger UI and API documentation endpoints
        if (context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/hangfire"))
        {
            await _next(context); // Skip API key check for Swagger
            return;
        }

        // Check for the API key in the request header
        if (!context.Request.Headers.ContainsKey("X-Api-Key") ||
            context.Request.Headers["X-Api-Key"] != _apiKey)
        {
            // If the API key is missing or incorrect, return unauthorized
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: API key is missing or invalid.");
            return;
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
