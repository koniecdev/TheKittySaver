using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class GetAdoptionAnnouncement : IEndpoint
{
    internal sealed record Query(AdoptionAnnouncementId AdoptionAnnouncementId) : IQuery<Result<AdoptionAnnouncementDetailsResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<AdoptionAnnouncementDetailsResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<AdoptionAnnouncementDetailsResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            AdoptionAnnouncementDetailsResponse? response = await _readDbContext.AdoptionAnnouncements
                .Where(aa => aa.Id == query.AdoptionAnnouncementId)
                .Select(aa => new AdoptionAnnouncementDetailsResponse(
                    Id: aa.Id,
                    PersonId: aa.PersonId,
                    Title: string.Join(", ", aa.Cats.OrderBy(x=>x.Name).Select(c => c.Name)),
                    Username: aa.Person.Username,
                    Description: aa.Description,
                    AddressCountryCode: aa.AddressCountryCode,
                    AddressPostalCode: aa.AddressPostalCode,
                    AddressRegion: aa.AddressRegion,
                    AddressCity: aa.AddressCity,
                    AddressLine: aa.AddressLine,
                    Email: aa.Email,
                    PhoneNumber: aa.PhoneNumber,
                    Status: aa.Status))
                .FirstOrDefaultAsync(cancellationToken);

            return response is null
                ? Result.Failure<AdoptionAnnouncementDetailsResponse>(
                    DomainErrors.AdoptionAnnouncementEntity.NotFound(query.AdoptionAnnouncementId))
                : Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("adoption-announcements/{adoptionAnnouncementId:guid}", async (
            Guid adoptionAnnouncementId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new AdoptionAnnouncementId(adoptionAnnouncementId));

            Result<AdoptionAnnouncementDetailsResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
