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

internal sealed class UpdateCatVaccination : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        VaccinationId VaccinationId,
        VaccinationType Type,
        DateOnly VaccinationDate,
        string? VeterinarianNote) : ICommand<Result<CatVaccinationResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatVaccinationResponse>>
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

        public async ValueTask<Result<CatVaccinationResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<CatVaccinationResponse>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Result updateTypeResult = cat.UpdateVaccinationType(command.VaccinationId, command.Type);
            if (updateTypeResult.IsFailure)
            {
                return Result.Failure<CatVaccinationResponse>(updateTypeResult.Error);
            }

            DateOnly currentDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
            Result<VaccinationDate> createDateResult = VaccinationDate.Create(command.VaccinationDate, currentDate);
            if (createDateResult.IsFailure)
            {
                return Result.Failure<CatVaccinationResponse>(createDateResult.Error);
            }

            Result updateDateResult = cat.UpdateVaccinationDate(command.VaccinationId, createDateResult.Value);
            if (updateDateResult.IsFailure)
            {
                return Result.Failure<CatVaccinationResponse>(updateDateResult.Error);
            }

            VaccinationNote? veterinarianNote = null;
            if (!string.IsNullOrWhiteSpace(command.VeterinarianNote))
            {
                Result<VaccinationNote> createNoteResult = VaccinationNote.Create(command.VeterinarianNote);
                if (createNoteResult.IsFailure)
                {
                    return Result.Failure<CatVaccinationResponse>(createNoteResult.Error);
                }
                veterinarianNote = createNoteResult.Value;
            }

            Result updateNoteResult = cat.UpdateVaccinationVeterinarianNote(command.VaccinationId, veterinarianNote);
            if (updateNoteResult.IsFailure)
            {
                return Result.Failure<CatVaccinationResponse>(updateNoteResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Vaccination vaccination = cat.Vaccinations.First(v => v.Id == command.VaccinationId);

            CatVaccinationResponse response = new(
                Id: vaccination.Id,
                CatId: cat.Id,
                Type: vaccination.Type,
                VaccinationDate: vaccination.Date.Value,
                VeterinarianNote: vaccination.VeterinarianNote?.Value);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("cats/{catId:guid}/vaccinations/{vaccinationId:guid}", async (
            Guid catId,
            Guid vaccinationId,
            UpdateCatVaccinationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new CatId(catId), new VaccinationId(vaccinationId));

            Result<CatVaccinationResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        });
    }
}

internal static class UpdateCatVaccinationMappings
{
    extension(UpdateCatVaccinationRequest request)
    {
        public UpdateCatVaccination.Command MapToCommand(CatId catId, VaccinationId vaccinationId)
        {
            Ensure.NotEmpty(catId);
            Ensure.NotEmpty(vaccinationId);
            ArgumentNullException.ThrowIfNull(request);
            
            UpdateCatVaccination.Command command = new(
                CatId: catId,
                VaccinationId: vaccinationId,
                Type: request.Type,
                VaccinationDate: request.VaccinationDate,
                VeterinarianNote: request.VeterinarianNote);
            return command;
        }
    }
}
