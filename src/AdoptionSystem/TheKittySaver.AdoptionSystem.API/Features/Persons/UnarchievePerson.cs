using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
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

internal sealed class UnarchievePerson : IEndpoint
{
    internal sealed record Command(
        IdentityId IdentityId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ICatRepository _catRepository;
        private readonly IAdoptionAnnouncementRepository _announcementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            ICatRepository catRepository,
            IAdoptionAnnouncementRepository announcementRepository)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _catRepository = catRepository;
            _announcementRepository = announcementRepository;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Person> maybePerson = 
                await _personRepository.GetByIdentityIdAsync(command.IdentityId, cancellationToken);
            if (maybePerson.HasNoValue)
            {
                return Result.Failure(DomainErrors.PersonEntity.NotFoundByIdentityId(command.IdentityId));
            }

            Person person = maybePerson.Value;
            person.Unarchive();
            
            IReadOnlyCollection<Cat> catsToUnarchieve = 
                await _catRepository.GetArchivedCatsByPersonIdAsync(person.Id, cancellationToken);
            foreach (Cat cat in catsToUnarchieve)
            {
                cat.Unarchive();
            }
            
            IReadOnlyCollection<AdoptionAnnouncement> announcementsToUnarchieve = 
                await _announcementRepository.GetArchivedAnnouncementsByPersonIdAsync(person.Id, cancellationToken);
            foreach (AdoptionAnnouncement announcement in announcementsToUnarchieve)
            {
                announcement.Unarchive();
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/unarchieve", async (
            UnarchievePersonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = new(IdentityId.Create(request.IdentityId));

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
