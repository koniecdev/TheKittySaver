using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class GetAdoptionAnnouncements : IEndpoint
{
    internal sealed record Query() : IQuery<Result<IReadOnlyList<AdoptionAnnouncementResponse>>>;

    internal sealed class Handler : IQueryHandler<Query, Result<IReadOnlyList<AdoptionAnnouncementResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<IReadOnlyList<AdoptionAnnouncementResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            IReadOnlyList<AdoptionAnnouncementResponse> response = await _readDbContext.AdoptionAnnouncements
                .Select(a => new AdoptionAnnouncementResponse(
                    Id: a.Id,
                    PersonId: a.PersonId,
                    Description: a.Description,
                    AddressCountryCode: a.AddressCountryCode,
                    AddressPostalCode: a.AddressPostalCode,
                    AddressRegion: a.AddressRegion,
                    AddressCity: a.AddressCity,
                    AddressLine: a.AddressLine,
                    Email: a.Email,
                    PhoneNumber: a.PhoneNumber,
                    Status: a.Status))
                .ToListAsync(cancellationToken);

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("adoption-announcements", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new();

            Result<IReadOnlyList<AdoptionAnnouncementResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
