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
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class AssignCat : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        AdoptionAnnouncementId AdoptionAnnouncementId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly ICatRepository _catRepository;
        private readonly ICatAdoptionAnnouncementAssignmentService _assignmentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            ICatRepository catRepository,
            ICatAdoptionAnnouncementAssignmentService assignmentService,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _catRepository = catRepository;
            _assignmentService = assignmentService;
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

            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);
            if (maybeAnnouncement.HasNoValue)
            {
                return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.NotFound(command.AdoptionAnnouncementId));
            }

            IReadOnlyCollection<Cat> catsAlreadyAssigned = await _catRepository.GetCatsByAdoptionAnnouncementIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);

            Result assignmentResult = _assignmentService.AssignCatToAdoptionAnnouncement(
                maybeCat.Value,
                maybeAnnouncement.Value,
                catsAlreadyAssigned,
                _timeProvider.GetUtcNow());

            if (assignmentResult.IsFailure)
            {
                return assignmentResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("cats/{catId:guid}/assignment", async (
            Guid catId,
            AssignCatRequest request,
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

internal static class AssignCatMappings
{
    extension(AssignCatRequest request)
    {
        public AssignCat.Command MapToCommand(CatId catId)
        {
            Ensure.NotEmpty(catId);
            ArgumentNullException.ThrowIfNull(request);
            
            AssignCat.Command command = new(
                CatId: catId,
                AdoptionAnnouncementId: request.AdoptionAnnouncementId);
            return command;
        }
    }
}
