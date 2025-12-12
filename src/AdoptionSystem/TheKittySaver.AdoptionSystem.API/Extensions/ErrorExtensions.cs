using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

internal static class ErrorExtensions
{
    public static ProblemDetails ToProblemDetails(this Error error)
    {
        ProblemDetails response = error.Type switch
        {
            TypeOfError.NotFound => CreateProblemDetails(error, StatusCodes.Status404NotFound),
            TypeOfError.Validation => CreateProblemDetails(error, StatusCodes.Status400BadRequest),
            TypeOfError.Conflict => CreateProblemDetails(error, StatusCodes.Status409Conflict),
            _ => CreateProblemDetails(error, StatusCodes.Status500InternalServerError)
        };
        return response;
    }

    public static int ToStatusCode(this Error error)
    {
        return error.Type switch
        {
            TypeOfError.NotFound => StatusCodes.Status404NotFound,
            TypeOfError.Validation => StatusCodes.Status400BadRequest,
            TypeOfError.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ProblemDetails CreateProblemDetails(Error error, int statusCode) =>
        new()
        {
            Status = statusCode,
            Title = GetTitle(error.Type),
            Detail = error.Message,
            Type = GetTypeUri(statusCode),
            Extensions = { ["errorCode"] = error.Code }
        };

    private static string GetTitle(TypeOfError type) =>
        type switch
        {
            TypeOfError.NotFound => "Not Found",
            TypeOfError.Validation => "Validation Error",
            TypeOfError.Conflict => "Conflict",
            TypeOfError.Failure => "Internal Server Error",
            _ => "Error"
        };

    private static string GetTypeUri(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
}
