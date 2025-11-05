using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess 
            ? new OkResult() 
            : result.Error.ToProblemDetails();

    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess 
            ? onSuccess(result.Value) 
            : result.Error.ToProblemDetails();

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result, 
        Func<T, string> locationFactory) =>
        result.IsSuccess
            ? new CreatedResult(locationFactory(result.Value), result.Value)
            : result.Error.ToProblemDetails();

    private static ObjectResult ToProblemDetails(this Error error) =>
        error.Type switch
        {
            TypeOfError.NotFound => new NotFoundObjectResult(CreateProblemDetails(error, StatusCodes.Status404NotFound)),
            TypeOfError.Validation => new BadRequestObjectResult(CreateProblemDetails(error, StatusCodes.Status400BadRequest)),
            TypeOfError.Conflict => new ConflictObjectResult(CreateProblemDetails(error, StatusCodes.Status409Conflict)),
            _ => new ObjectResult(CreateProblemDetails(error, StatusCodes.Status500InternalServerError)) 
                { StatusCode = StatusCodes.Status500InternalServerError }
        };

    private static ProblemDetails CreateProblemDetails(Error error, int statusCode) =>
        new()
        {
            Status = statusCode,
            Title = GetTitle(error.Type),
            Detail = error.Message,
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
}