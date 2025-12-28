using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class CreatePerson : IEndpoint
{
    internal sealed record Command(
        IdentityId IdentityId,
        string Username,
        string Email,
        string PhoneNumber) : ICommand<Result<PersonId>>;

    internal sealed class Handler : ICommandHandler<Command, Result<PersonId>>
    {
        private readonly IPersonCreationService _personCreationService;
        private readonly IPhoneNumberFactory _phoneNumberFactory;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonCreationService personCreationService,
            IPhoneNumberFactory phoneNumberFactory,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork)
        {
            _personCreationService = personCreationService;
            _phoneNumberFactory = phoneNumberFactory;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Result<PersonId>> Handle(Command command, CancellationToken cancellationToken)
        {
            Result<Email> createEmailResult = Email.Create(command.Email);
            if (createEmailResult.IsFailure)
            {
                return Result.Failure<PersonId>(createEmailResult.Error);
            }

            Result<Username> createUsernameResult = Username.Create(command.Username);
            if (createUsernameResult.IsFailure)
            {
                return Result.Failure<PersonId>(createUsernameResult.Error);
            }

            Result<PhoneNumber> createPhoneNumberResult = _phoneNumberFactory.Create(command.PhoneNumber);
            if (createPhoneNumberResult.IsFailure)
            {
                return Result.Failure<PersonId>(createPhoneNumberResult.Error);
            }

            Result<Person> personCreationResult = await _personCreationService.CreateAsync(
                createUsernameResult.Value,
                createEmailResult.Value,
                createPhoneNumberResult.Value,
                command.IdentityId,
                cancellationToken);
            if (personCreationResult.IsFailure)
            {
                return Result.Failure<PersonId>(personCreationResult.Error);
            }

            Person person = personCreationResult.Value;

            _personRepository.Insert(person);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return person.Id;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons", async (
            CreatePersonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand();

            Result<PersonId> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/persons/{commandResult.Value}", commandResult.Value);
        });
    }
}

internal static class CreatePersonMappings
{
    extension(CreatePersonRequest request)
    {
        public CreatePerson.Command MapToCommand()
        {
            ArgumentNullException.ThrowIfNull(request);

            CreatePerson.Command response = new(IdentityId: request.IdentityId,
                Username: request.Username,
                Email: request.Email,
                PhoneNumber: request.PhoneNumber);
            return response;
        }
    }
}
