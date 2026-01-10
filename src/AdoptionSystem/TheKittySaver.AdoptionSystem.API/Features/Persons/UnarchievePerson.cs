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
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class UnarchievePerson : IEndpoint
{
    internal sealed record Command(IdentityId IdentityId) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ICatRepository _catRepository;
        private readonly IAdoptionAnnouncementRepository _announcementRepository;
        private readonly IPersonArchiveDomainService _personArchiveDomainService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            ICatRepository catRepository,
            IAdoptionAnnouncementRepository announcementRepository,
            IPersonArchiveDomainService personArchiveDomainService,
            IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _catRepository = catRepository;
            _announcementRepository = announcementRepository;
            _personArchiveDomainService = personArchiveDomainService;
            _unitOfWork = unitOfWork;
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

            IReadOnlyCollection<Cat> archivedCats =
                await _catRepository.GetArchivedCatsByPersonIdAsync(person.Id, cancellationToken);

            IReadOnlyCollection<AdoptionAnnouncement> archivedAnnouncements =
                await _announcementRepository.GetArchivedAnnouncementsByPersonIdAsync(person.Id, cancellationToken);

            Result unarchiveResult = _personArchiveDomainService.Unarchive(
                person,
                archivedCats,
                archivedAnnouncements);

            if (unarchiveResult.IsFailure)
            {
                return unarchiveResult;
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
            Command command = new(request.IdentityId);

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}
