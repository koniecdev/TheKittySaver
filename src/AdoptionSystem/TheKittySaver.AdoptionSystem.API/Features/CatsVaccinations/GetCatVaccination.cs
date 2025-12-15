using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Vaccinations;

internal sealed class GetCatVaccination : IEndpoint
{
    internal sealed record Query(
        CatId CatId,
        VaccinationId VaccinationId) : IQuery<Result<CatVaccinationResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<CatVaccinationResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<CatVaccinationResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            CatVaccinationResponse? response = await _readDbContext.Vaccinations
                .Where(v =>
                    v.CatId == query.CatId
                    && v.Id == query.VaccinationId)
                .Select(v => new CatVaccinationResponse(
                    Id: v.Id,
                    CatId: v.CatId,
                    Type: v.Type,
                    VaccinationDate: v.VaccinationDate,
                    VeterinarianNote: v.VeterinarianNote))
                .FirstOrDefaultAsync(cancellationToken);

            return response ?? Result.Failure<CatVaccinationResponse>(
                DomainErrors.VaccinationEntity.NotFound(query.VaccinationId));
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/vaccinations/{vaccinationId:guid}", async (
            Guid catId,
            Guid vaccinationId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new CatId(catId), new VaccinationId(vaccinationId));

            Result<CatVaccinationResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
