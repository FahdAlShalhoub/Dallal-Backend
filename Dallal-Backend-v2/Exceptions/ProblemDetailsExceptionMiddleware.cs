using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Dallal_Backend_v2.Exceptions;

public class ProblemDetailsExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ProblemDetailsExceptionMiddleware(
        RequestDelegate next,
        ILogger<ProblemDetailsExceptionMiddleware> logger,
        IWebHostEnvironment environment,
        ProblemDetailsFactory problemDetailsFactory
    )
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _problemDetailsFactory = problemDetailsFactory;
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
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(
            exception,
            "An unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId,
            context.Request.Path
        );

        var problemDetails = CreateProblemDetails(context, exception, traceId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? 500;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment(),
            DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull,
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception,
        string traceId
    )
    {
        var (statusCode, title, detail, type) = GetErrorDetails(exception);

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: statusCode,
            title: title,
            detail: detail,
            type: type,
            instance: context.Request.Path
        );

        // Add custom extensions
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        // Add validation errors if applicable
        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.ValidationResult;
        }

        // Add stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        return problemDetails;
    }

    private static (int statusCode, string title, string detail, string type) GetErrorDetails(
        Exception exception
    )
    {
        return exception switch
        {
            ValidationException validationEx => (
                400,
                "Validation Error",
                "One or more validation errors occurred.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),

            UnauthorizedAccessException => (
                401,
                "Unauthorized",
                "Access to the requested resource is denied.",
                "https://tools.ietf.org/html/rfc7235#section-3.1"
            ),

            ArgumentNullException or ArgumentException => (
                400,
                "Bad Request",
                "The request contains invalid parameters.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),

            EntityNotFoundException or KeyNotFoundException => (
                404,
                "Not Found",
                "The requested resource was not found.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            ),

            TimeoutException => (
                408,
                "Request Timeout",
                "The server timed out waiting for the request.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.7"
            ),

            InvalidOperationException => (
                409,
                "Conflict",
                "The request could not be completed due to a conflict.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            ),

            _ => (
                500,
                "Internal Server Error",
                "An error occurred while processing your request.",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            ),
        };
    }
}
