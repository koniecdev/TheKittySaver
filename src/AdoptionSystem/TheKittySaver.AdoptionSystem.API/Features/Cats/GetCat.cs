using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class GetCat : IEndpoint
{
    internal sealed record Query(CatId CatId) : IQuery<Result<CatResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<CatResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<CatResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            CatReadModel? cat = await _readDbContext.Cats
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == query.CatId, cancellationToken);

            if (cat is null)
            {
                return Result.Failure<CatResponse>(DomainErrors.CatEntity.NotFound(query.CatId));
            }

            CatResponse response = new(
                Id: cat.Id,
                PersonId: cat.PersonId,
                AdoptionAnnouncementId: cat.AdoptionAnnouncementId,
                Name: cat.Name,
                Description: cat.Description,
                Age: cat.Age,
                Gender: cat.Gender,
                Color: cat.Color,
                WeightValueInKilograms: cat.WeightValueInKilograms,
                HealthStatus: cat.HealthStatus,
                SpecialNeedsStatusHasSpecialNeeds: cat.SpecialNeedsStatusHasSpecialNeeds,
                SpecialNeedsStatusDescription: cat.SpecialNeedsStatusDescription,
                SpecialNeedsStatusSeverityType: cat.SpecialNeedsStatusSeverityType,
                Temperament: cat.Temperament,
                AdoptionHistoryReturnCount: cat.AdoptionHistoryReturnCount,
                AdoptionHistoryLastReturnDate: cat.AdoptionHistoryLastReturnDate,
                AdoptionHistoryLastReturnReason: cat.AdoptionHistoryLastReturnReason,
                ListingSourceType: cat.ListingSourceType,
                ListingSourceSourceName: cat.ListingSourceSourceName,
                IsNeutered: cat.NeuteringStatusIsNeutered,
                InfectiousDiseaseStatusFivStatus: cat.InfectiousDiseaseStatusFivStatus,
                InfectiousDiseaseStatusFelvStatus: cat.InfectiousDiseaseStatusFelvStatus,
                InfectiousDiseaseStatusLastTestedAt: cat.InfectiousDiseaseStatusLastTestedAt);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new CatId(catId));

            Result<CatResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
