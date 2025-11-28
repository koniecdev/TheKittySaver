using Bogus;
using NSubstitute;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Person = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Person;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Services;

public sealed class PersonCreationServiceTests
{
    private static readonly Faker Faker = new();
    private readonly IPersonUniquenessCheckerService _uniquenessChecker;
    private readonly PersonCreationService _service;

    public PersonCreationServiceTests()
    {
        _uniquenessChecker = Substitute.For<IPersonUniquenessCheckerService>();
        _service = new PersonCreationService(_uniquenessChecker);
    }


    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenAllDataIsValidAndUnique()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        Result<Person> result = await _service.CreateAsync(
            username,
            email,
            phoneNumber,
            createdAt,
            identityId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Username.ShouldBe(username);
        result.Value.Email.ShouldBe(email);
        result.Value.PhoneNumber.ShouldBe(phoneNumber);
        result.Value.IdentityId.ShouldBe(identityId);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallUniquenessChecker_ForBothEmailAndPhone()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        await _service.CreateAsync(username, email, phoneNumber, createdAt, identityId);

        //Assert
        await _uniquenessChecker.Received(1).IsEmailTakenAsync(email, Arg.Any<CancellationToken>());
        await _uniquenessChecker.Received(1).IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task CreateAsync_ShouldFail_WhenEmailIsAlreadyTaken()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Email is taken

        //Act
        Result<Person> result = await _service.CreateAsync(
            username,
            email,
            phoneNumber,
            createdAt,
            identityId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.EmailAlreadyTaken(email));
    }

    [Fact]
    public async Task CreateAsync_ShouldNotCheckPhoneNumber_WhenEmailIsAlreadyTaken()
    {
        //Arrange - Optimization: don't check phone if email fails first
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        //Act
        await _service.CreateAsync(username, email, phoneNumber, createdAt, identityId);

        //Assert
        await _uniquenessChecker.Received(1).IsEmailTakenAsync(email, Arg.Any<CancellationToken>());
        await _uniquenessChecker.DidNotReceive().IsPhoneNumberTakenAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenPhoneNumberIsAlreadyTaken()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false)); // Email is unique
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Phone is taken

        //Act
        Result<Person> result = await _service.CreateAsync(
            username,
            email,
            phoneNumber,
            createdAt,
            identityId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.PhoneNumberAlreadyTaken(phoneNumber));
    }

    [Fact]
    public async Task CreateAsync_ShouldCheckPhoneNumber_OnlyAfterEmailCheckSucceeds()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        await _service.CreateAsync(username, email, phoneNumber, createdAt, identityId);

        //Assert - Verify both checks were called
        await _uniquenessChecker.Received(1).IsEmailTakenAsync(email, Arg.Any<CancellationToken>());
        await _uniquenessChecker.Received(1).IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldFailWithEmailError_WhenBothEmailAndPhoneTaken()
    {
        //Arrange - Email check fails first, so we return email error
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        //Act
        Result<Person> result = await _service.CreateAsync(
            username,
            email,
            phoneNumber,
            createdAt,
            identityId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.EmailAlreadyTaken(email));
    }

    [Fact]
    public async Task CreateAsync_ShouldPassCancellationToken_ToUniquenessChecker()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _uniquenessChecker.IsEmailTakenAsync(email, cancellationToken)
            .Returns(Task.FromResult(false));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, cancellationToken)
            .Returns(Task.FromResult(false));

        //Act
        await _service.CreateAsync(username, email, phoneNumber, createdAt, identityId, cancellationToken);

        //Assert
        await _uniquenessChecker.Received(1).IsEmailTakenAsync(email, cancellationToken);
        await _uniquenessChecker.Received(1).IsPhoneNumberTakenAsync(phoneNumber, cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_ShouldDelegateToPersonCreate_WhenUniquenessChecksPass()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();
        CreatedAt createdAt = CreateDefaultCreatedAt();
        IdentityId identityId = IdentityId.New();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        Result<Person> result = await _service.CreateAsync(
            username,
            email,
            phoneNumber,
            createdAt,
            identityId);

        //Assert - Verify Person.Create() was successful
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<Person>();
        result.Value.CreatedAt.ShouldBe(createdAt);
    }

    private static Username CreateRandomUsername()
    {
        Result<Username> result = Username.Create(Faker.Person.UserName);
        result.EnsureSuccess();
        return result.Value;
    }

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

    private static CreatedAt CreateDefaultCreatedAt()
    {
        Result<CreatedAt> result = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        result.EnsureSuccess();
        return result.Value;
    }
}
