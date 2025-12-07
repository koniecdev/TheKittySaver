using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class GetCats : IEndpoint
{
    internal sealed record Query : IQuery<Result<IReadOnlyList<CatResponse>>>;

    internal sealed class Handler : IQueryHandler<Query, Result<IReadOnlyList<CatResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<IReadOnlyList<CatResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            IReadOnlyList<CatResponse> response = await _readDbContext.Cats
                .Select(c => new CatResponse(
                    Id: c.Id,
                    PersonId: c.PersonId,
                    AdoptionAnnouncementId: c.AdoptionAnnouncementId,
                    Name: c.Name,
                    Description: c.Description,
                    Age: c.Age,
                    Gender: c.Gender,
                    Color: c.Color,
                    WeightValueInKilograms: c.WeightValueInKilograms,
                    HealthStatus: c.HealthStatus,
                    SpecialNeedsStatusHasSpecialNeeds: c.SpecialNeedsStatusHasSpecialNeeds,
                    SpecialNeedsStatusDescription: c.SpecialNeedsStatusDescription,
                    SpecialNeedsStatusSeverityType: c.SpecialNeedsStatusSeverityType,
                    Temperament: c.Temperament,
                    AdoptionHistoryReturnCount: c.AdoptionHistoryReturnCount,
                    AdoptionHistoryLastReturnDate: c.AdoptionHistoryLastReturnDate,
                    AdoptionHistoryLastReturnReason: c.AdoptionHistoryLastReturnReason,
                    ListingSourceType: c.ListingSourceType,
                    ListingSourceSourceName: c.ListingSourceSourceName,
                    IsNeutered: c.NeuteringStatusIsNeutered,
                    InfectiousDiseaseStatusFivStatus: c.InfectiousDiseaseStatusFivStatus,
                    InfectiousDiseaseStatusFelvStatus: c.InfectiousDiseaseStatusFelvStatus,
                    InfectiousDiseaseStatusLastTestedAt: c.InfectiousDiseaseStatusLastTestedAt))
                .ToListAsync(cancellationToken);
            
            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new();

            Result<IReadOnlyList<CatResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
