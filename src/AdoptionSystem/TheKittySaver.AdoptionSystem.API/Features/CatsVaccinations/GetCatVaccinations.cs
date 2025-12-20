using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsVaccinations;

internal sealed class GetCatVaccinations : IEndpoint
{
    internal sealed record Query(CatId CatId) : IQuery<Result<IReadOnlyList<CatVaccinationResponse>>>;

    internal sealed class Handler : IQueryHandler<Query, Result<IReadOnlyList<CatVaccinationResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<IReadOnlyList<CatVaccinationResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            bool catExists = await _readDbContext.Cats
                .AnyAsync(c => c.Id == query.CatId, cancellationToken);

            if (!catExists)
            {
                return Result.Failure<IReadOnlyList<CatVaccinationResponse>>(
                    DomainErrors.CatEntity.NotFound(query.CatId));
            }

            IReadOnlyList<CatVaccinationResponse> response = await _readDbContext.Vaccinations
                .Where(v => v.CatId == query.CatId)
                .OrderByDescending(v => v.VaccinationDate)
                .Select(v => new CatVaccinationResponse(
                    Id: v.Id,
                    CatId: v.CatId,
                    Type: v.Type,
                    VaccinationDate: v.VaccinationDate,
                    VeterinarianNote: v.VeterinarianNote))
                .ToListAsync(cancellationToken);

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/vaccinations", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new CatId(catId));

            Result<IReadOnlyList<CatVaccinationResponse>> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
