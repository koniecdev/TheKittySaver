using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementReassignmentServices;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class ReassignCat : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        AdoptionAnnouncementId AdoptionAnnouncementId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICatRepository _catRepository;
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly ICatAdoptionAnnouncementReassignmentService _reassignmentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            ICatRepository catRepository,
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            ICatAdoptionAnnouncementReassignmentService reassignmentService,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _catRepository = catRepository;
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _reassignmentService = reassignmentService;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            if (cat.AdoptionAnnouncementId is null)
            {
                return Result.Failure(DomainErrors.CatEntity.Assignment.NotAssignedToAdoptionAnnouncement(command.CatId));
            }

            if (cat.AdoptionAnnouncementId == command.AdoptionAnnouncementId)
            {
                return Result.Failure(DomainErrors.CatEntity.Assignment.CannotReassignToSameAnnouncement(command.CatId));
            }

            Maybe<AdoptionAnnouncement> maybeSourceAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                cat.AdoptionAnnouncementId.Value,
                cancellationToken);
            if (maybeSourceAnnouncement.HasNoValue)
            {
                return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.NotFound(cat.AdoptionAnnouncementId.Value));
            }

            Maybe<AdoptionAnnouncement> maybeDestinationAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);
            if (maybeDestinationAnnouncement.HasNoValue)
            {
                return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.NotFound(command.AdoptionAnnouncementId));
            }

            if (cat.PersonId == maybeDestinationAnnouncement.Value.PersonId)
            {
                return Result.Failure(DomainErrors.CatEntity.Assignment.CannotReassignToSameOwner(command.CatId));
            }
            
            IReadOnlyCollection<Cat> catsInDestination = await _catRepository.GetCatsByAdoptionAnnouncementIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);

            Result reassignResult = _reassignmentService.ReassignCatToAnotherAdoptionAnnouncement(
                cat,
                maybeSourceAnnouncement.Value,
                maybeDestinationAnnouncement.Value,
                catsInDestination,
                _timeProvider.GetUtcNow());

            if (reassignResult.IsFailure)
            {
                return reassignResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("cats/{catId:guid}/assignment", async (
            Guid catId,
            ReassignCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new CatId(catId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}

internal static class ReassignCatMappings
{
    extension(ReassignCatRequest request)
    {
        public ReassignCat.Command MapToCommand(CatId catId)
        {
            ReassignCat.Command command = new(
                CatId: catId,
                AdoptionAnnouncementId: request.AdoptionAnnouncementId);
            return command;
        }
    }
}
