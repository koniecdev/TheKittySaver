using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class DeletePerson : IEndpoint
{
    internal sealed record Command(PersonId PersonId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ICatRepository _catRepository;
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            ICatRepository catRepository,
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _catRepository = catRepository;
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
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

            int catsCount = await _catRepository.CountCatsByPersonIdAsync(person.Id, cancellationToken);
            if (catsCount > 0)
            {
                return Result.Failure(DomainErrors.PersonEntity.HasRelatedEntities(person.Id));
            }

            int announcementsCount = await _adoptionAnnouncementRepository.CountAnnouncementsByPersonIdAsync(person.Id, cancellationToken);
            if (announcementsCount > 0)
            {
                return Result.Failure(DomainErrors.PersonEntity.HasRelatedEntities(person.Id));
            }

            _personRepository.Remove(person);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{personId:guid}", async (
            Guid personId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(new PersonId(personId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
