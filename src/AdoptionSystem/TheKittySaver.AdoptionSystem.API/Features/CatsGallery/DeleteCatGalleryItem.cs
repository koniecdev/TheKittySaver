using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Gallery;

internal sealed class DeleteCatGalleryItem : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        CatGalleryItemId GalleryItemId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICatRepository _catRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatFileStorage _catFileStorage;

        public Handler(
            ICatRepository catRepository,
            IUnitOfWork unitOfWork,
            ICatFileStorage catFileStorage)
        {
            _catRepository = catRepository;
            _unitOfWork = unitOfWork;
            _catFileStorage = catFileStorage;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Result deleteResult = cat.RemoveGalleryItem(command.GalleryItemId);
            if (deleteResult.IsFailure)
            {
                return deleteResult;
            }

            Result deleteFileResult = await _catFileStorage.DeleteGalleryItemAsync(
                command.CatId,
                command.GalleryItemId,
                cancellationToken);

            if (deleteFileResult.IsFailure)
            {
                return deleteFileResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("cats/{catId:guid}/gallery/{galleryItemId:guid}", async (
            Guid catId,
            Guid galleryItemId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new CatId(catId), new CatGalleryItemId(galleryItemId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
