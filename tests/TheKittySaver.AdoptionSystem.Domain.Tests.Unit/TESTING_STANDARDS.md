# Unit Testing Standards

> **Purpose:** This document defines consistent standards for all unit tests in the Domain layer.
> All tests should follow these conventions for maintainability and readability.

---

## 📋 Table of Contents

1. [File Organization](#file-organization)
2. [Naming Conventions](#naming-conventions)
3. [Test Structure](#test-structure)
4. [Helper Methods & Factories](#helper-methods--factories)
5. [Mocking](#mocking)
6. [Assertions](#assertions)
7. [Examples](#examples)

---

## 1. File Organization

### ✅ Standard Structure

```csharp
using ...;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatAssignmentTests
{
    // 1. Static/Instance Fields
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset TestDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

    // 2. Test Groups (use #region)
    #region Happy Path Tests

    [Fact]
    public void Method_ShouldSucceed_WhenCondition()
    {
        // tests...
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Method_ShouldFail_WhenInvalidData()
    {
        // tests...
    }

    #endregion

    // 3. Helper Methods (always at the end)
    #region Helper Methods

    private static Cat CreateTestCat()
    {
        // helper logic...
    }

    #endregion
}
```

### 🎯 Rules

1. **Always use `#region`** to group related tests
2. **Helper methods always last**, in their own `#region Helper Methods`
3. **Static fields** for test data (unless you need mocks/state)
4. **Sealed classes** - test classes should be `public sealed`

---

## 2. Naming Conventions

### ✅ Test Method Names

**Pattern:** `MethodName_ShouldExpectation_WhenCondition`

```csharp
// ✅ Good
[Fact]
public void AssignToAdoptionAnnouncement_ShouldSucceed_WhenCatIsDraft()

[Fact]
public void AssignToAdoptionAnnouncement_ShouldFail_WhenCatHasNoThumbnail()

[Fact]
public void Create_ShouldReturnFailure_WhenTestDateIsInFuture()

// ❌ Bad
[Fact]
public void TestAssignment() // What does it test?

[Fact]
public void Assign_Success() // Missing condition

[Fact]
public void ShouldFailWhenInvalid() // Missing method name
```

### ✅ Test Class Names

**Pattern:** `{AggregateOrService}+{Feature}Tests`

```csharp
// ✅ Good
CatAssignmentTests
CatGalleryManagementTests
PersonCreationServiceTests
InfectiousDiseaseStatusTests

// ❌ Bad
CatTests // Too broad
AssignmentTests // Missing aggregate name
TestCat // Wrong suffix
```

### ✅ #region Names

**Pattern:** `{Category} Tests` or `{Category}`

```csharp
#region Happy Path Tests
#region Validation Tests
#region PersonId Validation Tests
#region Infectious Disease Compatibility Tests - CRITICAL
#region Domain Events Tests
#region Helper Methods
```

---

## 3. Test Structure

### ✅ AAA Pattern (Arrange-Act-Assert)

**Always use comments to separate sections:**

```csharp
[Fact]
public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenAllConditionsAreMet()
{
    //Arrange
    PersonId personId = PersonId.New();
    Cat cat = CreateDraftCatWithThumbnail(personId);
    AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

    //Act
    Result result = _service.AssignCatToAdoptionAnnouncement(
        cat,
        announcement,
        Array.Empty<Cat>(),
        OperationDate);

    //Assert
    result.IsSuccess.ShouldBeTrue();
    cat.Status.ShouldBe(CatStatusType.Published);
    cat.AdoptionAnnouncementId.ShouldBe(announcement.Id);
}
```

### 🎯 Rules

1. **Always include `//Arrange`, `//Act`, `//Assert` comments**
2. **Arrange:** Set up test data
3. **Act:** Execute ONE operation
4. **Assert:** Verify expectations (multiple assertions OK if related)

### ✅ Explanatory Comments

**Add comments in Arrange when scenario is not obvious:**

```csharp
[Fact]
public void AssignCat_ShouldFail_WhenFivPositiveMixesWithFivNegative()
{
    //Arrange - FIV+ cat cannot mix with FIV- cat
    PersonId personId = PersonId.New();
    InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);
    InfectiousDiseaseStatus fivNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

    // Rest of test...
}
```

**Don't add comments for obvious scenarios:**

```csharp
[Fact]
public void UpdateName_ShouldUpdateName_WhenValidNameIsProvided()
{
    //Arrange
    Cat cat = CatFactory.CreateRandom(Faker);
    CatName newName = CatFactory.CreateRandomName(Faker);

    // No comment needed - test name explains everything
}
```

---

## 4. Helper Methods & Factories

### ✅ When to Create Helper Methods

**Create helpers when:**
- ✅ Setup code repeats 3+ times
- ✅ Setup is complex (5+ lines)
- ✅ Setup requires specific state

**Don't create helpers when:**
- ❌ Used only once
- ❌ Makes test less readable
- ❌ Factory already exists

### ✅ Helper Method Pattern

```csharp
#region Helper Methods

// Use descriptive names
private static Cat CreateDraftCatWithThumbnail(PersonId personId)
{
    return CatFactory.CreateWithThumbnail(Faker, personId: personId);
}

private static AdoptionAnnouncement CreateActiveAnnouncement(PersonId personId)
{
    return AdoptionAnnouncementFactory.CreateRandom(Faker, personId: personId);
}

private static InfectiousDiseaseStatus CreateDiseaseStatus(FivStatus fivStatus, FelvStatus felvStatus)
{
    DateOnly currentDate = new(2025, 6, 1);
    DateOnly testDate = new(2025, 5, 1);

    Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
        fivStatus,
        felvStatus,
        testDate,
        currentDate);

    result.EnsureSuccess();
    return result.Value;
}

#endregion
```

### 🎯 Rules

1. **Static** for stateless helpers
2. **Private** (only used in this test class)
3. **Descriptive names** - explain what they create
4. **Use `.EnsureSuccess()`** to fail fast if setup fails

---

## 5. Mocking

### ✅ When to Use Mocks

**Use mocks for:**
- ✅ Domain Services (external dependencies)
- ✅ Repository interfaces
- ✅ Infrastructure services

**Don't mock:**
- ❌ Value Objects (use real instances)
- ❌ Entities (use Factory)
- ❌ Domain logic

### ✅ Mock Setup Pattern

**Use constructor injection for mocks:**

```csharp
public sealed class PersonCreationServiceTests
{
    private readonly IPersonUniquenessCheckerService _uniquenessChecker;
    private readonly PersonCreationService _service;

    public PersonCreationServiceTests()
    {
        _uniquenessChecker = Substitute.For<IPersonUniquenessCheckerService>();
        _service = new PersonCreationService(_uniquenessChecker);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenEmailIsAlreadyTaken()
    {
        //Arrange
        Email email = CreateRandomEmail();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        //Act
        Result<Person> result = await _service.CreateAsync(...);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.EmailAlreadyTaken(email));
    }
}
```

### ✅ Mock Verification

**Use NSubstitute's `.Received()` to verify interactions:**

```csharp
//Assert
await _uniquenessChecker.Received(1).IsEmailTakenAsync(email, Arg.Any<CancellationToken>());
await _uniquenessChecker.DidNotReceive().IsPhoneNumberTakenAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>());
```

---

## 6. Assertions

### ✅ Use Shouldly

**We use Shouldly for fluent assertions:**

```csharp
// ✅ Good - Shouldly
result.IsSuccess.ShouldBeTrue();
result.IsFailure.ShouldBeTrue();
cat.Status.ShouldBe(CatStatusType.Published);
cat.AdoptionAnnouncementId.ShouldNotBeNull();
person.Addresses.Count.ShouldBe(1);
events.ShouldContain(e => e is CatClaimedDomainEvent);

// ❌ Bad - Assert.*
Assert.True(result.IsSuccess);
Assert.Equal(CatStatusType.Published, cat.Status);
```

### ✅ Exception Assertions

```csharp
[Fact]
public void UpdateName_ShouldThrow_WhenNullNameIsProvided()
{
    //Arrange
    Cat cat = CatFactory.CreateRandom(Faker);

    //Act
    Action updateName = () => cat.UpdateName(null!);

    //Assert
    updateName.ShouldThrow<ArgumentNullException>()
        .ParamName?.ToLower().ShouldContain("name");
}
```

### ✅ Domain Event Assertions

```csharp
[Fact]
public void Claim_ShouldRaiseCatClaimedDomainEvent_WhenSuccessful()
{
    //Arrange & Act
    // ... cat.Claim(...)

    //Assert
    IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
    events.ShouldContain(e => e is CatClaimedDomainEvent);

    CatClaimedDomainEvent evt = events.OfType<CatClaimedDomainEvent>().First();
    evt.CatId.ShouldBe(cat.Id);
    evt.AdoptionAnnouncementId.ShouldBe(cat.AdoptionAnnouncementId);
}
```

---

## 7. Examples

### ✅ Example 1: Aggregate Tests (with #region)

```csharp
using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatAssignmentTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset ValidOperationDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

    #region Happy Path Tests

    [Fact]
    public void AssignToAdoptionAnnouncement_ShouldAssign_WhenCatIsDraftWithThumbnail()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.AdoptionAnnouncementId.ShouldBe(announcementId);
        cat.Status.ShouldBe(CatStatusType.Published);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void AssignToAdoptionAnnouncement_ShouldFail_WhenCatHasNoThumbnail()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.ThumbnailProperty.RequiredForPublishing(cat.Id));
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    public void Reassign_ShouldRaiseDomainEvent_WhenSuccessful()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        cat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), ValidOperationDate);

        AdoptionAnnouncementId newAnnouncementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.ReassignToAnotherAdoptionAnnouncement(newAnnouncementId, ValidOperationDate.AddDays(1));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldContain(e => e is CatReassignedToAnotherAnnouncementDomainEvent);
    }

    #endregion
}
```

### ✅ Example 2: Service Tests (with mocks)

```csharp
using Bogus;
using NSubstitute;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

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

    #region Happy Path Tests

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenAllDataIsValidAndUnique()
    {
        //Arrange
        Username username = CreateRandomUsername();
        Email email = CreateRandomEmail();
        PhoneNumber phoneNumber = CreateRandomPhoneNumber();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _uniquenessChecker.IsPhoneNumberTakenAsync(phoneNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act
        Result<Person> result = await _service.CreateAsync(username, email, phoneNumber, ...);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe(email);
    }

    #endregion

    #region Email Uniqueness Validation Tests

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenEmailIsAlreadyTaken()
    {
        //Arrange
        Email email = CreateRandomEmail();

        _uniquenessChecker.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        //Act
        Result<Person> result = await _service.CreateAsync(...);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.PersonEntity.EmailAlreadyTaken(email));
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
```

### ✅ Example 3: Value Object Tests (with Theory)

```csharp
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class InfectiousDiseaseStatusTests
{
    private static readonly DateOnly CurrentDate = new(2025, 6, 1);
    private static readonly DateOnly ValidTestDate = new(2024, 1, 15);

    #region IsCompatibleWith Tests - FIV Status

    [Theory]
    [InlineData(FivStatus.Positive, FivStatus.Positive, true)]
    [InlineData(FivStatus.Positive, FivStatus.Negative, false)]
    [InlineData(FivStatus.Positive, FivStatus.NotTested, true)]
    [InlineData(FivStatus.Negative, FivStatus.Negative, true)]
    [InlineData(FivStatus.NotTested, FivStatus.NotTested, true)]
    public void IsCompatibleWith_ShouldReturnExpectedResult_ForFivStatusCombinations(
        FivStatus status1,
        FivStatus status2,
        bool expectedCompatibility)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(fivStatus: status1);
        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(fivStatus: status2);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBe(expectedCompatibility);
    }

    #endregion

    #region Helper Methods

    private static InfectiousDiseaseStatus CreateDiseaseStatus(
        FivStatus fivStatus = FivStatus.Negative,
        FelvStatus felvStatus = FelvStatus.Negative)
    {
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            ValidTestDate,
            CurrentDate);

        result.EnsureSuccess();
        return result.Value;
    }

    #endregion
}
```

---

## 📝 Checklist for New Tests

Before submitting a test class, verify:

- [ ] Class is `public sealed`
- [ ] Static fields for Faker and test dates
- [ ] Tests grouped with `#region`
- [ ] Helper methods in `#region Helper Methods` at the end
- [ ] Test names follow `MethodName_ShouldExpectation_WhenCondition`
- [ ] AAA pattern with comments (`//Arrange`, `//Act`, `//Assert`)
- [ ] Using Shouldly for assertions
- [ ] Explanatory comments where needed (not everywhere)
- [ ] Mocks set up in constructor (if needed)
- [ ] No direct use of `Assert.*` (use Shouldly)

---

## 🔄 Updating Existing Tests

When updating existing tests:

1. ✅ Add `#region` grouping if missing
2. ✅ Move helper methods to end in `#region Helper Methods`
3. ✅ Verify naming follows standard
4. ✅ Add AAA comments if missing
5. ✅ Convert `Assert.*` to Shouldly
6. ❌ Don't rewrite tests that already work
7. ❌ Don't break existing functionality

---

## 🎯 Why These Standards?

1. **#region** - Easy navigation in large test files (463+ lines)
2. **Consistent naming** - Know what test does without reading body
3. **AAA comments** - Clear separation of concerns
4. **Helper methods last** - Test logic first, helpers second
5. **Static fields** - Shared test data without state
6. **Shouldly** - Better error messages than Assert.*

---

## 🚫 Anti-Patterns to Avoid

### ❌ Don't Create God Test Classes

```csharp
// ❌ Bad - too broad
public class CatTests { }

// ✅ Good - focused
public class CatAssignmentTests { }
public class CatGalleryManagementTests { }
public class CatVaccinationManagementTests { }
```

### ❌ Don't Test Implementation Details

```csharp
// ❌ Bad - tests private state
[Fact]
public void UpdateName_ShouldSetPrivateField_WhenCalled()

// ✅ Good - tests behavior
[Fact]
public void UpdateName_ShouldUpdateName_WhenValidNameIsProvided()
```

### ❌ Don't Over-Mock

```csharp
// ❌ Bad - mocking Value Object
var name = Substitute.For<CatName>();

// ✅ Good - use real instance
Result<CatName> nameResult = CatName.Create("Fluffy");
CatName name = nameResult.Value;
```

### ❌ Don't Skip AAA Comments

```csharp
// ❌ Bad - hard to read
[Fact]
public void Test()
{
    Cat cat = CatFactory.CreateRandom(Faker);
    Result result = cat.UpdateName(newName);
    result.IsSuccess.ShouldBeTrue();
}

// ✅ Good - clear structure
[Fact]
public void UpdateName_ShouldSucceed_WhenValidName()
{
    //Arrange
    Cat cat = CatFactory.CreateRandom(Faker);
    CatName newName = CatFactory.CreateRandomName(Faker);

    //Act
    Result result = cat.UpdateName(newName);

    //Assert
    result.IsSuccess.ShouldBeTrue();
}
```

---

## 📚 Resources

- **Shouldly Documentation:** https://docs.shouldly.org/
- **NSubstitute Documentation:** https://nsubstitute.github.io/
- **xUnit Documentation:** https://xunit.net/
- **AAA Pattern:** https://automationpanda.com/2020/07/07/arrange-act-assert-a-pattern-for-writing-good-tests/

---

**Last Updated:** 2025-12-01
**Maintained By:** Development Team
