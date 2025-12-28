using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsVaccinations;

internal sealed class CreateCatVaccination : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        VaccinationType Type,
        DateOnly VaccinationDate,
        string? VeterinarianNote) : ICommand<Result<VaccinationId>>;

    internal sealed class Handler : ICommandHandler<Command, Result<VaccinationId>>
    {
        private readonly ICatRepository _catRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            ICatRepository catRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _catRepository = catRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result<VaccinationId>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<VaccinationId>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            VaccinationNote? veterinarianNote = null;
            if (!string.IsNullOrWhiteSpace(command.VeterinarianNote))
            {
                Result<VaccinationNote> createNoteResult = VaccinationNote.Create(command.VeterinarianNote);
                if (createNoteResult.IsFailure)
                {
                    return Result.Failure<VaccinationId>(createNoteResult.Error);
                }
                veterinarianNote = createNoteResult.Value;
            }

            DateTimeOffset dateTimeOfOperation = _timeProvider.GetUtcNow();

            Result<Vaccination> addVaccinationResult = cat.AddVaccination(
                command.Type,
                command.VaccinationDate,
                dateTimeOfOperation,
                veterinarianNote);

            if (addVaccinationResult.IsFailure)
            {
                return Result.Failure<VaccinationId>(addVaccinationResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(addVaccinationResult.Value.Id);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("cats/{catId:guid}/vaccinations", async (
            Guid catId,
            CreateCatVaccinationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new CatId(catId));

            Result<VaccinationId> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/cats/{catId}/vaccinations/{commandResult}", commandResult.Value);
        });
    }
}

internal static class CreateCatVaccinationMappings
{
    extension(CreateCatVaccinationRequest request)
    {
        public CreateCatVaccination.Command MapToCommand(CatId catId)
        {
            Ensure.NotEmpty(catId);
            ArgumentNullException.ThrowIfNull(request);

            CreateCatVaccination.Command command = new(
                CatId: catId,
                Type: request.Type,
                VaccinationDate: request.VaccinationDate,
                VeterinarianNote: request.VeterinarianNote);
            return command;
        }
    }
}
