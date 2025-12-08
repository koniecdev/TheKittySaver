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

internal sealed class CreateCatGalleryItem : IEndpoint
{
    internal sealed record Command(CatId CatId) : ICommand<Result<CatGalleryItemResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatGalleryItemResponse>>
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

        public async ValueTask<Result<CatGalleryItemResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<CatGalleryItemResponse>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Result<CatGalleryItemId> addGalleryItemResult = cat.AddGalleryItem();
            if (addGalleryItemResult.IsFailure)
            {
                return Result.Failure<CatGalleryItemResponse>(addGalleryItemResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            CatGalleryItem galleryItem = cat.GalleryItems.First(g => g.Id == addGalleryItemResult.Value);

            CatGalleryItemResponse response = new(
                Id: galleryItem.Id,
                CatId: cat.Id,
                DisplayOrder: galleryItem.DisplayOrder.Value);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("cats/{catId:guid}/gallery", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new CatId(catId));

            Result<CatGalleryItemResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/cats/{catId}/gallery/{commandResult.Value.Id}", commandResult.Value);
        });
    }
}
