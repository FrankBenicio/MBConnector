using System.Net;
using System.Text.Json;

namespace MBConnector.Api.Middlewares
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ArgumentNullException or ArgumentException => new ErrorResponse(HttpStatusCode.BadRequest, exception.Message),
                UnauthorizedAccessException => new ErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized access."),
                TimeoutException => new ErrorResponse(HttpStatusCode.RequestTimeout, "Request timed out."),
                _ => new ErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        private sealed record ErrorResponse(HttpStatusCode StatusCode, string Message);
    }
}