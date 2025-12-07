using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class GetAdoptionAnnouncement : IEndpoint
{
    internal sealed record Query(AdoptionAnnouncementId AdoptionAnnouncementId) : IQuery<Result<AdoptionAnnouncementResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<AdoptionAnnouncementResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<AdoptionAnnouncementResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            AdoptionAnnouncementResponse? response = await _readDbContext.AdoptionAnnouncements
                .Where(announcement => announcement.Id == query.AdoptionAnnouncementId)
                .Select(announcement => new AdoptionAnnouncementResponse(
                    Id: announcement.Id,
                    PersonId: announcement.PersonId,
                    Description: announcement.Description,
                    AddressCountryCode: announcement.AddressCountryCode,
                    AddressPostalCode: announcement.AddressPostalCode,
                    AddressRegion: announcement.AddressRegion,
                    AddressCity: announcement.AddressCity,
                    AddressLine: announcement.AddressLine,
                    Email: announcement.Email,
                    PhoneNumber: announcement.PhoneNumber,
                    Status: announcement.Status))
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(
                    DomainErrors.AdoptionAnnouncementErrors.NotFound(query.AdoptionAnnouncementId));
            }
            
            return response;
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

            Result<AdoptionAnnouncementResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
