using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;

namespace TheKittySaver.AdoptionSystem.API.Extensions;


internal static class ErrorExtensions
{
    extension(Error error)
    {
        public ProblemDetails ToProblemDetails()
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

        private ProblemDetails CreateProblemDetails(int statusCode) =>
            new()
            {
                Status = statusCode,
                Type = $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{statusCode}",
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
}
