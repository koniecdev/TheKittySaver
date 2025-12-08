using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Gallery;

internal sealed class UpsertCatThumbnail : IEndpoint
{
    internal sealed record Command(CatId CatId) : ICommand<Result<CatThumbnailResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatThumbnailResponse>>
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

        public async ValueTask<Result<CatThumbnailResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<CatThumbnailResponse>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Result<CatThumbnailId> upsertResult = cat.UpsertThumbnail();
            if (upsertResult.IsFailure)
            {
                return Result.Failure<CatThumbnailResponse>(upsertResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            CatThumbnailResponse response = new(
                Id: upsertResult.Value,
                CatId: cat.Id);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("cats/{catId:guid}/thumbnail", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new CatId(catId));

            Result<CatThumbnailResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        });
    }
}
