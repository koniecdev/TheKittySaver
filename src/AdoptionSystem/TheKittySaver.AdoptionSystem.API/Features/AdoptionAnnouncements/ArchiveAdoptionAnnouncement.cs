using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class ArchiveAdoptionAnnouncement : IEndpoint
{
    internal sealed record Command(AdoptionAnnouncementId AdoptionAnnouncementId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IAdoptionAnnouncementArchiveDomainService _archiveDomainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IAdoptionAnnouncementArchiveDomainService archiveDomainService,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _archiveDomainService = archiveDomainService;
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
                return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.NotFound(command.AdoptionAnnouncementId));
            }

            AdoptionAnnouncement announcement = maybeAnnouncement.Value;

            Result<ArchivedAt> archivedAtResult = ArchivedAt.Create(_timeProvider.GetUtcNow());
            if (archivedAtResult.IsFailure)
            {
                return archivedAtResult;
            }

            Result archiveResult = _archiveDomainService.Archive(announcement, archivedAtResult.Value);
            if (archiveResult.IsFailure)
            {
                return archiveResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("adoption-announcements/{adoptionAnnouncementId:guid}/archive", async (
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
