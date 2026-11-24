using EquipmentBorrowing.Api.Contracts;

namespace EquipmentBorrowing.Api.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger)
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
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or cancelled the request.
            _logger.LogInformation(
                "HTTP request cancelled. Trace ID: {TraceId}",
                context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Unhandled API failure. Type: {ExceptionType}. " +
                "Trace ID: {TraceId}",
                exception.GetType().Name,
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();

            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                HttpMethods.IsGet(context.Request.Method)
                    ? new ApiError("server_error", "The server could not complete the request.")
                    : new ApiError("outcome_unknown", "The operation could not be confirmed. Refresh records before retrying."));
        }
    }
}