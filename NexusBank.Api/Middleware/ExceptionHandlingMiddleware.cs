using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NexusBank.Domain.Exceptions;

namespace NexusBank.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        var (statusCode, problem) = exception switch
        {
            ValidationException ve      => (StatusCodes.Status422UnprocessableEntity, BuildValidationProblem(ve)),
            NotFoundException           => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Title  = "Not Found",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound,
            }),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Title  = "Unauthorized",
                Detail = exception.Message,
                Status = StatusCodes.Status401Unauthorized,
            }),
            ConflictException           => (StatusCodes.Status409Conflict, new ProblemDetails
            {
                Title  = "Conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict,
            }),
            _                           => (StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title  = "An unexpected error occurred",
                Detail = "An internal server error occurred.",
                Status = StatusCodes.Status500InternalServerError,
            }),
        };

        if (correlationId is not null)
            problem.Extensions["correlationId"] = correlationId;

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
        else
            logger.LogInformation(exception, "Client error {StatusCode} {ExceptionType}. CorrelationId: {CorrelationId}",
                statusCode, exception.GetType().Name, correlationId);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static HttpValidationProblemDetails BuildValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new HttpValidationProblemDetails(errors)
        {
            Title  = "Validation failed",
            Status = StatusCodes.Status422UnprocessableEntity,
        };
    }
}
