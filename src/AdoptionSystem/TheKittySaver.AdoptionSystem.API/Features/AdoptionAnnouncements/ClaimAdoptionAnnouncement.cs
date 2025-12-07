using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class ClaimAdoptionAnnouncement : IEndpoint
{
    internal sealed record Command(AdoptionAnnouncementId AdoptionAnnouncementId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);

            if (maybeAnnouncement.HasNoValue)
            {
                return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.NotFound(command.AdoptionAnnouncementId));
            }

            AdoptionAnnouncement announcement = maybeAnnouncement.Value;

            Result<ClaimedAt> createClaimedAtResult = ClaimedAt.Create(_timeProvider.GetUtcNow());
            if (createClaimedAtResult.IsFailure)
            {
                return Result.Failure(createClaimedAtResult.Error);
            }

            Result claimResult = announcement.Claim(createClaimedAtResult.Value);
            if (claimResult.IsFailure)
            {
                return claimResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("adoption-announcements/{adoptionAnnouncementId:guid}/claim", async (
            Guid adoptionAnnouncementId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new AdoptionAnnouncementId(adoptionAnnouncementId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
