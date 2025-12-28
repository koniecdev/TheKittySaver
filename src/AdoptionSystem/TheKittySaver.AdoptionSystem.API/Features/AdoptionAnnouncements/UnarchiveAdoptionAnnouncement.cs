using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class UnarchiveAdoptionAnnouncement : IEndpoint
{
    internal sealed record Command(AdoptionAnnouncementId AdoptionAnnouncementId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IAdoptionAnnouncementArchiveDomainService _archiveDomainService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IAdoptionAnnouncementArchiveDomainService archiveDomainService,
            IUnitOfWork unitOfWork)
        {
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _archiveDomainService = archiveDomainService;
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetArchivedByIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);

            if (maybeAnnouncement.HasNoValue)
            {
                return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.NotFound(command.AdoptionAnnouncementId));
            }

            AdoptionAnnouncement announcement = maybeAnnouncement.Value;

            Result unarchiveResult = _archiveDomainService.Unarchive(announcement);
            if (unarchiveResult.IsFailure)
            {
                return unarchiveResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("adoption-announcements/{adoptionAnnouncementId:guid}/unarchive", async (
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
