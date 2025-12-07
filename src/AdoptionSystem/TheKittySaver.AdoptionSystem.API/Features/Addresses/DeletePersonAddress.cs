using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Addresses;

internal sealed class DeletePersonAddress : IEndpoint
{
    internal sealed record Command(
        PersonId PersonId,
        AddressId AddressId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
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

            Result deleteResult = person.DeleteAddress(command.AddressId);
            if (deleteResult.IsFailure)
            {
                return deleteResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{personId:guid}/addresses/{addressId:guid}", async (
            Guid personId,
            Guid addressId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new PersonId(personId), new AddressId(addressId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
