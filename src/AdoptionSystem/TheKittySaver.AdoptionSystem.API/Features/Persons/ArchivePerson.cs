using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class ArchivePerson : IEndpoint
{
    internal sealed record Command(PersonId PersonId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ICatRepository _catRepository;
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IPersonArchiveDomainService _personArchiveDomainService;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            ICatRepository catRepository,
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IPersonArchiveDomainService personArchiveDomainService,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _personRepository = personRepository;
            _catRepository = catRepository;
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _personArchiveDomainService = personArchiveDomainService;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (maybePerson.HasNoValue)
            {
                return Result.Failure(DomainErrors.PersonEntity.NotFound(command.PersonId));
            }

            Person person = maybePerson.Value;

            IReadOnlyCollection<AdoptionAnnouncement> announcements =
                await _adoptionAnnouncementRepository
                    .GetAdoptionAnnouncementsByPersonIdAsync(person.Id, cancellationToken);

            IReadOnlyCollection<Cat> cats =
                await _catRepository.GetCatsByPersonIdAsync(person.Id, cancellationToken);

            Result<ArchivedAt> archivedAtResult = ArchivedAt.Create(_timeProvider.GetUtcNow());
            if (archivedAtResult.IsFailure)
            {
                return archivedAtResult;
            }

            Result archiveResult = _personArchiveDomainService.Archive(
                person,
                cats,
                announcements,
                archivedAtResult.Value);

            if (archiveResult.IsFailure)
            {
                return archiveResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/archive", async (
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
