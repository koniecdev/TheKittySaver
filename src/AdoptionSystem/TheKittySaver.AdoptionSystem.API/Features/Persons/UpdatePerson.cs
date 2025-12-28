using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class UpdatePerson : IEndpoint
{
    internal sealed record Command(
        PersonId PersonId,
        string Username,
        string Email,
        string PhoneNumber) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPersonUpdateService _personUpdateService;
        private readonly IPhoneNumberFactory _phoneNumberFactory;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            IPersonUpdateService personUpdateService,
            IPhoneNumberFactory phoneNumberFactory,
            IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _personUpdateService = personUpdateService;
            _phoneNumberFactory = phoneNumberFactory;
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (maybePerson.HasNoValue)
            {
                return Result.Failure(DomainErrors.PersonEntity.NotFound(command.PersonId));
            }

            Person person = maybePerson.Value;

            Result<Email> createEmailResult = Email.Create(command.Email);
            if (createEmailResult.IsFailure)
            {
                return Result.Failure(createEmailResult.Error);
            }

            Result<Username> createUsernameResult = Username.Create(command.Username);
            if (createUsernameResult.IsFailure)
            {
                return Result.Failure(createUsernameResult.Error);
            }

            Result<PhoneNumber> createPhoneNumberResult = _phoneNumberFactory.Create(command.PhoneNumber);
            if (createPhoneNumberResult.IsFailure)
            {
                return Result.Failure(createPhoneNumberResult.Error);
            }

            Result updateUsernameResult = person.UpdateUsername(createUsernameResult.Value);
            if (updateUsernameResult.IsFailure)
            {
                return updateUsernameResult;
            }

            Result updateEmailResult = await _personUpdateService.UpdateEmailAsync(
                command.PersonId,
                createEmailResult.Value,
                cancellationToken);
            if (updateEmailResult.IsFailure)
            {
                return updateEmailResult;
            }

            Result updatePhoneNumberResult = await _personUpdateService.UpdatePhoneNumberAsync(
                command.PersonId,
                createPhoneNumberResult.Value,
                cancellationToken);
            if (updatePhoneNumberResult.IsFailure)
            {
                return updatePhoneNumberResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}", async (
            Guid personId,
            UpdatePersonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new PersonId(personId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}

internal static class UpdatePersonMappings
{
    extension(UpdatePersonRequest request)
    {
        public UpdatePerson.Command MapToCommand(PersonId personId)
        {
            Ensure.NotEmpty(personId);
            ArgumentNullException.ThrowIfNull(request);

            UpdatePerson.Command command = new(
                PersonId: personId,
                Username: request.Username,
                Email: request.Email,
                PhoneNumber: request.PhoneNumber);
            return command;
        }
    }
}
