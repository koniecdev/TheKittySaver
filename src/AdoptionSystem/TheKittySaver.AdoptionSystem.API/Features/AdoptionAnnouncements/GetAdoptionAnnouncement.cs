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
            AdoptionAnnouncementReadModel? announcement = await _readDbContext.AdoptionAnnouncements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == query.AdoptionAnnouncementId, cancellationToken);

            if (announcement is null)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(
                    DomainErrors.AdoptionAnnouncementErrors.NotFound(query.AdoptionAnnouncementId));
            }

            AdoptionAnnouncementResponse response = new(
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
                Status: announcement.Status);

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
