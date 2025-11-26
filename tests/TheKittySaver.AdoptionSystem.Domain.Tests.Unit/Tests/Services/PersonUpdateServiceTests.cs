using Bogus;
using NSubstitute;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Services;

public sealed class PersonUpdateServiceTests
{
    private static readonly Faker Faker = new();
    private readonly IPersonRepository _personRepository;
    private readonly IPersonUniquenessCheckerService _uniquenessChecker;
    private readonly PersonUpdateService _service;

    public PersonUpdateServiceTests()
    {
        _personRepository = Substitute.For<IPersonRepository>();
        _uniquenessChecker = Substitute.For<IPersonUniquenessCheckerService>();
        _service = new PersonUpdateService(_personRepository, _uniquenessChecker);
    }

    #region UpdateEmailAsync - Happy Path Tests

    [Fact]
    public async Task UpdateEmailAsync_ShouldSucceed_WhenEmailIsUniqueAndPersonExists()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Email newEmail = CreateRandomEmail();

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsEmailTakenAsync(newEmail, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        Result result = await _service.UpdateEmailAsync(person.Id, newEmail);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Email.ShouldBe(newEmail);
    }

    [Fact]
    public async Task UpdateEmailAsync_ShouldSucceed_WhenUpdatingToSameEmail()
    {
        //Arrange - Same email should skip uniqueness check
        Person person = PersonFactory.CreateRandom(Faker);
        Email sameEmail = person.Email;

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));

        //Act
        Result result = await _service.UpdateEmailAsync(person.Id, sameEmail);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        await _uniquenessChecker.DidNotReceive().IsEmailTakenAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateEmailAsync - Person Not Found Tests

    [Fact]
    public async Task UpdateEmailAsync_ShouldFail_WhenPersonNotFound()
    {
        //Arrange
        PersonId nonExistentPersonId = PersonId.New();
        Email newEmail = CreateRandomEmail();

        _personRepository.GetByIdAsync(nonExistentPersonId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.None));

        //Act
        Result result = await _service.UpdateEmailAsync(nonExistentPersonId, newEmail);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.NotFound(nonExistentPersonId));
    }

    #endregion

    #region UpdateEmailAsync - Email Uniqueness Tests

    [Fact]
    public async Task UpdateEmailAsync_ShouldFail_WhenNewEmailIsAlreadyTaken()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Email newEmail = CreateRandomEmail();

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsEmailTakenAsync(newEmail, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Email is taken

        //Act
        Result result = await _service.UpdateEmailAsync(person.Id, newEmail);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.EmailAlreadyTaken(newEmail));
        person.Email.ShouldNotBe(newEmail); // Email should not change
    }

    [Fact]
    public async Task UpdateEmailAsync_ShouldNotCheckUniqueness_WhenEmailDidNotChange()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Email sameEmail = person.Email;

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));

        //Act
        await _service.UpdateEmailAsync(person.Id, sameEmail);

        //Assert - Uniqueness check should be skipped for same email
        await _uniquenessChecker.DidNotReceive().IsEmailTakenAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdatePhoneNumberAsync - Happy Path Tests

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldSucceed_WhenPhoneNumberIsUniqueAndPersonExists()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber newPhoneNumber = CreateRandomPhoneNumber();

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsPhoneNumberTakenAsync(newPhoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        Result result = await _service.UpdatePhoneNumberAsync(person.Id, newPhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.PhoneNumber.ShouldBe(newPhoneNumber);
    }

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldSucceed_WhenUpdatingToSamePhoneNumber()
    {
        //Arrange - Same phone number should skip uniqueness check
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber samePhoneNumber = person.PhoneNumber;

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));

        //Act
        Result result = await _service.UpdatePhoneNumberAsync(person.Id, samePhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        await _uniquenessChecker.DidNotReceive().IsPhoneNumberTakenAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdatePhoneNumberAsync - Person Not Found Tests

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldFail_WhenPersonNotFound()
    {
        //Arrange
        PersonId nonExistentPersonId = PersonId.New();
        PhoneNumber newPhoneNumber = CreateRandomPhoneNumber();

        _personRepository.GetByIdAsync(nonExistentPersonId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.None));

        //Act
        Result result = await _service.UpdatePhoneNumberAsync(nonExistentPersonId, newPhoneNumber);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.NotFound(nonExistentPersonId));
    }

    #endregion

    #region UpdatePhoneNumberAsync - Phone Number Uniqueness Tests

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldFail_WhenNewPhoneNumberIsAlreadyTaken()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber newPhoneNumber = CreateRandomPhoneNumber();

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsPhoneNumberTakenAsync(newPhoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Phone number is taken

        //Act
        Result result = await _service.UpdatePhoneNumberAsync(person.Id, newPhoneNumber);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.PhoneNumberAlreadyTaken(newPhoneNumber));
        person.PhoneNumber.ShouldNotBe(newPhoneNumber); // Phone should not change
    }

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldNotCheckUniqueness_WhenPhoneNumberDidNotChange()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber samePhoneNumber = person.PhoneNumber;

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));

        //Act
        await _service.UpdatePhoneNumberAsync(person.Id, samePhoneNumber);

        //Assert - Uniqueness check should be skipped for same phone number
        await _uniquenessChecker.DidNotReceive().IsPhoneNumberTakenAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task UpdateEmailAsync_ShouldPassCancellationToken_ToRepositoryAndUniquenessChecker()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Email newEmail = CreateRandomEmail();
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _personRepository.GetByIdAsync(person.Id, cancellationToken)
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsEmailTakenAsync(newEmail, cancellationToken)
            .Returns(Task.FromResult(false));

        //Act
        await _service.UpdateEmailAsync(person.Id, newEmail, cancellationToken);

        //Assert
        await _personRepository.Received(1).GetByIdAsync(person.Id, cancellationToken);
        await _uniquenessChecker.Received(1).IsEmailTakenAsync(newEmail, cancellationToken);
    }

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldPassCancellationToken_ToRepositoryAndUniquenessChecker()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber newPhoneNumber = CreateRandomPhoneNumber();
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _personRepository.GetByIdAsync(person.Id, cancellationToken)
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsPhoneNumberTakenAsync(newPhoneNumber, cancellationToken)
            .Returns(Task.FromResult(false));

        //Act
        await _service.UpdatePhoneNumberAsync(person.Id, newPhoneNumber, cancellationToken);

        //Assert
        await _personRepository.Received(1).GetByIdAsync(person.Id, cancellationToken);
        await _uniquenessChecker.Received(1).IsPhoneNumberTakenAsync(newPhoneNumber, cancellationToken);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task UpdateEmailAsync_ShouldRetrievePersonFromRepository_BeforeUpdating()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Email newEmail = CreateRandomEmail();

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsEmailTakenAsync(newEmail, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        await _service.UpdateEmailAsync(person.Id, newEmail);

        //Assert
        await _personRepository.Received(1).GetByIdAsync(person.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdatePhoneNumberAsync_ShouldRetrievePersonFromRepository_BeforeUpdating()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber newPhoneNumber = CreateRandomPhoneNumber();

        _personRepository.GetByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Maybe<Person>.From(person)));
        _uniquenessChecker.IsPhoneNumberTakenAsync(newPhoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        await _service.UpdatePhoneNumberAsync(person.Id, newPhoneNumber);

        //Assert
        await _personRepository.Received(1).GetByIdAsync(person.Id, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Helper Methods

    private static Email CreateRandomEmail()
    {
        Result<Email> result = Email.Create(Faker.Person.Email);
        result.EnsureSuccess();
        return result.Value;
    }

    private static PhoneNumber CreateRandomPhoneNumber()
    {
        return PhoneNumber.CreateUnsafe(Faker.Person.Phone);
    }

    #endregion
}
