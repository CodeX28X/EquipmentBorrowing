using System.Text.Json;
using System.Text.Json.Serialization;
using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Api.Extensions;
using EquipmentBorrowing.Api.Middleware;
using EquipmentBorrowing.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configure HTTP controllers and JSON contracts.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.Strict;

        options.JsonSerializerOptions.UnmappedMemberHandling =
            JsonUnmappedMemberHandling.Disallow;
    });

// Standardize automatic validation responses.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(
            new ApiError(
                "validation_error",
                "The submitted request contains invalid " +
                "or missing fields."));
    };

    options.SuppressMapClientErrors = true;
});

// Register machine-readable API documentation.
builder.Services.AddOpenApi();

// Register existing application and PostgreSQL services.
builder.Services.AddEquipmentBorrowingServices(
    builder.Configuration);

builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddSingleton<ApplicationClock>();

var app = builder.Build();

// Handle unexpected failures safely.
app.UseMiddleware<ApiExceptionMiddleware>();

// Provide consistent error bodies for unmatched routes
// and unsupported HTTP methods.
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;

    ApiError error = response.StatusCode switch
    {
        400 => new ApiError(
            "validation_error",
            "The request is invalid."),

        404 => new ApiError(
            "not_found",
            "The requested resource was not found."),

        405 => new ApiError(
            "method_not_allowed",
            "The HTTP method is not supported."),

        500 => new ApiError(
            "server_error",
            "The server could not complete the request."),

        _ => new ApiError(
            "http_error",
            "The request could not be completed.")
    };

    await response.WriteAsJsonAsync(error);
});

// Make the OpenAPI document independently retrievable.
app.MapOpenApi("/openapi/generated/{documentName}.json");
app.MapGet("/openapi/v1.json", () => Results.File(
    Path.Combine(AppContext.BaseDirectory, "openapi.lab5.json"),
    "application/json")).ExcludeFromDescription();

// Register all API controllers.
app.MapControllers();

app.Run();