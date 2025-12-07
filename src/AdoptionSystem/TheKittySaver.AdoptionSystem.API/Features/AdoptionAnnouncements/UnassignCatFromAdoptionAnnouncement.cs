using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class UnassignCatFromAdoptionAnnouncement : IEndpoint
{
    internal sealed record Command(
        AdoptionAnnouncementId AdoptionAnnouncementId,
        CatId CatId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly ICatRepository _catRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            ICatRepository catRepository,
            IUnitOfWork unitOfWork)
        {
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _catRepository = catRepository;
            _unitOfWork = unitOfWork;
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

            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            if (cat.AdoptionAnnouncementId != command.AdoptionAnnouncementId)
            {
                return Result.Failure(DomainErrors.CatEntity.Assignment.NotAssignedToAdoptionAnnouncement(command.CatId));
            }

            Result unassignResult = cat.UnassignFromAdoptionAnnouncement();
            if (unassignResult.IsFailure)
            {
                return unassignResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("adoption-announcements/{adoptionAnnouncementId:guid}/cats/{catId:guid}", async (
            Guid adoptionAnnouncementId,
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(
                new AdoptionAnnouncementId(adoptionAnnouncementId),
                new CatId(catId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
