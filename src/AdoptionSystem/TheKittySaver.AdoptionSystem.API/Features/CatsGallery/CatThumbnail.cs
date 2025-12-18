using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsGallery;

internal sealed class GetCatThumbnail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/thumbnail", async (
            Guid catId,
            IApplicationReadDbContext readDbContext,
            ICatFileStorage catFileStorage,
            CancellationToken cancellationToken) =>
        {
            CatId catIdTyped = new(catId);

            CatThumbnailReadModel? thumbnail = await readDbContext.CatThumbnails
                .FirstOrDefaultAsync(t => t.CatId == catIdTyped, cancellationToken);

            if (thumbnail is null)
            {
                return Results.Problem(
                    DomainErrors.CatEntity.ThumbnailProperty.NotUploaded(catIdTyped).ToProblemDetails());
            }

            Result<CatFileData> fileResult = await catFileStorage.GetThumbnailAsync(
                catIdTyped,
                thumbnail.Id,
                cancellationToken);

            if (fileResult.IsFailure)
            {
                return Results.Problem(fileResult.Error.ToProblemDetails());
            }

            CatFileData fileData = fileResult.Value;
            return Results.File(fileData.FileStream, fileData.ContentType, fileData.FileName);
        });
    }
}

internal sealed class UpsertCatThumbnail : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        Stream FileStream,
        string ContentType) : ICommand<Result<CatThumbnailResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatThumbnailResponse>>
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

            Result saveFileResult = await _catFileStorage.SaveThumbnailAsync(
                command.CatId,
                upsertResult.Value,
                command.FileStream,
                command.ContentType,
                cancellationToken);

            if (saveFileResult.IsFailure)
            {
                return Result.Failure<CatThumbnailResponse>(saveFileResult.Error);
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
            IFormFile file,
            IFileUploadValidator fileUploadValidator,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result validationResult = fileUploadValidator.ValidateThumbnailFile(
                file.Length,
                file.ContentType,
                file.FileName);

            if (validationResult.IsFailure)
            {
                return Results.Problem(validationResult.Error.ToProblemDetails());
            }

            await using Stream fileStream = file.OpenReadStream();
            Command command = new(new CatId(catId), fileStream, file.ContentType);

            Result<CatThumbnailResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        })
        .DisableAntiforgery()
        .WithMetadata(new RequestSizeLimitAttribute(2 * 1024 * 1024)); // 2MB
    }
}

internal sealed class DeleteCatThumbnail : IEndpoint
{
    internal sealed record Command(CatId CatId) : ICommand<Result>;

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

            Result<CatThumbnailId> removeResult = cat.RemoveThumbnail();
            if (removeResult.IsFailure)
            {
                return Result.Failure(removeResult.Error);
            }

            Result deleteFileResult = await _catFileStorage.DeleteThumbnailAsync(
                command.CatId,
                removeResult.Value,
                cancellationToken);

            if (deleteFileResult.IsFailure)
            {
                return Result.Failure(deleteFileResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("cats/{catId:guid}/thumbnail", async (
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
