using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;

internal static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage response)
    {
        public async Task<string> EnsureSuccessWithDetailsAsync()
        {
            JsonSerializerOptions jsonOptions = new()
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            
            if (response.StatusCode is HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return content;
            }
            
            ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, jsonOptions);

            if (problemDetails is not null)
            {
                string errorMessage =
                    $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).\n" +
                    $"Title: {problemDetails.Title}\n" +
                    $"Detail: {problemDetails.Detail}\n" +
                    $"Type: {problemDetails.Type}\n";

                if (problemDetails.Extensions.Count <= 0)
                {
                    throw new HttpRequestException(errorMessage);
                }

                errorMessage += "Extensions:\n";
                foreach (KeyValuePair<string, object?> extension in problemDetails.Extensions)
                {
                    string extensionValue = JsonSerializer.Serialize(extension.Value, jsonOptions);
                    errorMessage += $"  {extension.Key}: {extensionValue}\n";
                }

                throw new HttpRequestException(errorMessage);
            }

            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).\n" +
                $"Content: {content}");
        }

        public async Task<ProblemDetails> ToProblemDetailsAsync()
        {
            ProblemDetails problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>()
                ?? throw new JsonException("Could not deserialize to problem details");
            return problemDetails;
        }
    }
}
