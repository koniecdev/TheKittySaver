using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsGallery;

internal sealed class ReorderCatGallery : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        IReadOnlyList<GalleryItemOrderEntry> NewOrders) : ICommand<Result<IReadOnlyList<CatGalleryItemResponse>>>;

    internal sealed class Handler : ICommandHandler<Command, Result<IReadOnlyList<CatGalleryItemResponse>>>
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

        public async ValueTask<Result<IReadOnlyList<CatGalleryItemResponse>>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<IReadOnlyList<CatGalleryItemResponse>>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrdersDictionary = new();
            foreach (GalleryItemOrderEntry entry in command.NewOrders)
            {
                Result<CatGalleryItemDisplayOrder> displayOrderResult = CatGalleryItemDisplayOrder.Create(
                    entry.DisplayOrder,
                    Cat.MaximumGalleryItemsCount);

                if (displayOrderResult.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<CatGalleryItemResponse>>(displayOrderResult.Error);
                }

                newOrdersDictionary[entry.GalleryItemId] = displayOrderResult.Value;
            }

            Result reorderResult = cat.ReorderGalleryItems(newOrdersDictionary);
            if (reorderResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<CatGalleryItemResponse>>(reorderResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            IReadOnlyList<CatGalleryItemResponse> response = cat.GalleryItems
                .OrderBy(g => g.DisplayOrder.Value)
                .Select(g => new CatGalleryItemResponse(
                    Id: g.Id,
                    CatId: cat.Id,
                    DisplayOrder: g.DisplayOrder.Value))
                .ToList();

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("cats/{catId:guid}/gallery/reorder", async (
            Guid catId,
            ReorderCatGalleryRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new CatId(catId));

            Result<IReadOnlyList<CatGalleryItemResponse>> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        });
    }
}

internal static class ReorderCatGalleryMappings
{
    extension(ReorderCatGalleryRequest request)
    {
        public ReorderCatGallery.Command MapToCommand(CatId catId)
        {
            ArgumentNullException.ThrowIfNull(request);

            ReorderCatGallery.Command command = new(
                CatId: catId,
                NewOrders: request.NewOrders);
            return command;
        }
    }
}
