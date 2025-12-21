using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class GetCat : IEndpoint
{
    internal sealed record Query(CatId CatId) : IQuery<Result<CatDetailsResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<CatDetailsResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<CatDetailsResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            CatDetailsResponse? response = await _readDbContext.Cats
                .Where(c => c.Id == query.CatId)
                .Select(cat => new CatDetailsResponse(
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
                    InfectiousDiseaseStatusLastTestedAt: cat.InfectiousDiseaseStatusLastTestedAt,
                    Vaccinations: cat.Vaccinations
                        .OrderByDescending(v => v.VaccinationDate)
                        .Select(v => new CatVaccinationEmbeddedDto(
                            Id: v.Id,
                            Type: v.Type,
                            VaccinationDate: v.VaccinationDate,
                            VeterinarianNote: v.VeterinarianNote))
                        .ToList(),
                    GalleryItems: cat.GalleryItems
                        .OrderBy(g => g.DisplayOrder)
                        .Select(g => new CatGalleryItemEmbeddedDto(
                            Id: g.Id,
                            DisplayOrder: g.DisplayOrder))
                        .ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return Result.Failure<CatDetailsResponse>(DomainErrors.CatEntity.NotFound(query.CatId));
            }

            return Result.Success(response);
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

            Result<CatDetailsResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
