using System.Net;
using System.Text.Json;
using HRS.API.Contracts.DTOs;

namespace HRS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ApiResponse<string>.Fail(
                "An unexpected error occurred.",
                new List<string> { ex.Message }
            );

            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
