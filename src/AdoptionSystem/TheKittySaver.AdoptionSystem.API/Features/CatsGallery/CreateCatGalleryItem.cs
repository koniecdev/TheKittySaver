using Mediator;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;
using TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsGallery;

internal sealed class CreateCatGalleryItem : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        Stream FileStream,
        string ContentType) : ICommand<Result<CatGalleryItemResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatGalleryItemResponse>>
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

        public async ValueTask<Result<CatGalleryItemResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<CatGalleryItemResponse>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Result<CatGalleryItem> addGalleryItemResult = cat.AddGalleryItem();
            if (addGalleryItemResult.IsFailure)
            {
                return Result.Failure<CatGalleryItemResponse>(addGalleryItemResult.Error);
            }

            CatGalleryItem catGalleryItem = addGalleryItemResult.Value;

            Result saveFileResult = await _catFileStorage.SaveGalleryItemAsync(
                command.CatId,
                catGalleryItem.Id,
                command.FileStream,
                command.ContentType,
                cancellationToken);

            if (saveFileResult.IsFailure)
            {
                return Result.Failure<CatGalleryItemResponse>(saveFileResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            CatGalleryItemResponse response = new(
                Id: catGalleryItem.Id,
                CatId: cat.Id,
                DisplayOrder: catGalleryItem.DisplayOrder.Value);

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("cats/{catId:guid}/gallery", async (
            Guid catId,
            IFormFile file,
            IFileUploadValidator fileUploadValidator,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result metadataValidation = fileUploadValidator.ValidateGalleryFile(
                file.Length,
                file.ContentType,
                file.FileName);

            if (metadataValidation.IsFailure)
            {
                return Results.Problem(metadataValidation.Error.ToProblemDetails());
            }

            await using MemoryStream memoryStream = new();
            await using (Stream uploadStream = file.OpenReadStream())
            {
                await uploadStream.CopyToAsync(memoryStream, cancellationToken);
            }

            memoryStream.Position = 0;

            Result contentValidation = await fileUploadValidator.ValidateGalleryFileWithContentAsync(
                memoryStream,
                file.Length,
                file.ContentType,
                file.FileName,
                cancellationToken);

            if (contentValidation.IsFailure)
            {
                return Results.Problem(contentValidation.Error.ToProblemDetails());
            }

            memoryStream.Position = 0;
            Command command = new(new CatId(catId), memoryStream, file.ContentType);

            Result<CatGalleryItemResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/cats/{catId}/gallery/{commandResult.Value.Id}", commandResult.Value);
        })
        .DisableAntiforgery()
        .WithRequestTimeout(TimeSpan.FromMinutes(2))
        .WithMetadata(new RequestSizeLimitAttribute(25 * 1024 * 1024)); // 25MB
    }
}
