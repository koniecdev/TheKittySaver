using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class DeleteCat : IEndpoint
{
    internal sealed record Command(CatId CatId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICatRepository _catRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            ICatRepository catRepository,
            IUnitOfWork unitOfWork)
        {
            _catRepository = catRepository;
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            switch (maybeCat.Value)
            {
                case {AdoptionAnnouncementId: not null, Status: CatStatusType.Claimed}:
                    return Result.Failure(DomainErrors.CatEntity.ClaimedCatRemoval);
                case {AdoptionAnnouncementId: not null, Status: not CatStatusType.Claimed}
                    when (await _catRepository.GetCatsByAdoptionAnnouncementIdAsync(
                        maybeCat.Value.AdoptionAnnouncementId.Value,
                        cancellationToken)).Count <= 1:
                    return Result.Failure(DomainErrors.CatEntity.TheOnlyAdoptionAnnouncementCatRemoval);
            }
            
            _catRepository.Remove(maybeCat.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("cats/{catId:guid}", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new CatId(catId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
