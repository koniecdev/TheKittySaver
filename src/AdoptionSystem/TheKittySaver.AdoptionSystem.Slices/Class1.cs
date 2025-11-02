using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TheKittySaver.AdoptionSystem.Slices;

public sealed class Dupa
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("test", async () =>
        {
            string dupa = await Task.FromResult("dupa");
            return Results.Ok(dupa);
        });
    }
}