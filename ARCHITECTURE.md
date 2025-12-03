# TheKittySaver - Domain Architecture Decisions

> **Context:** This is an MVP adoption system for cats. Design decisions prioritize
> consistency, maintainability, and pragmatism over academic "clean code" patterns.

---

## 🏗️ Core Design Patterns

### 1. Result Pattern Everywhere

**Decision:** All domain operations return `Result` or `Result<T>`, never throw exceptions.

```csharp
// ✅ Every operation
Result assignResult = cat.AssignToAdoptionAnnouncement(announcementId, dateTime);
if (assignResult.IsFailure)
    return assignResult.Error;

Result<CatGalleryItemId> addResult = cat.AddGalleryItem();
if (addResult.IsFailure)
    return addResult.Error;
```

**Why:**

- Predictable error handling - Always check `IsFailure`, zero surprises
- Railway-oriented programming - Easy to chain operations
- No hidden control flow - No try-catch scattered everywhere
- Explicit failure cases - Domain errors are first-class citizens

**Trade-off:**

- More boilerplate (`if (result.IsFailure)` checks)
- But: Explicitness > magic

### 2. Update* Methods Pattern

**Decision:** Every mutable aggregate property has an `UpdateX()` method instead of internal/public setters.

```csharp
// ✅ Consistent API
public Result UpdateName(CatName updatedName)
{
    ArgumentNullException.ThrowIfNull(updatedName);
    Name = updatedName;
    return Result.Success();
}

public Result UpdateAge(CatAge updatedAge) { /* same structure */ }
public Result UpdateColor(CatColor updatedColor) { /* same structure */ }
// ... 13 total methods in Cat aggregate
```

**Why:**

- Consistency - All mutations go through methods, not mix of methods/setters
- Future-proofing - Easy to add validation/business rules later without refactoring
- Explicit mutation points - Clear where state changes happen
- Same return type - All return `Result` for predictable error handling

**Example future-proofing:**

```csharp
// Today: simple setter
public Result UpdateName(CatName updatedName)
{
    ArgumentNullException.ThrowIfNull(updatedName);
    Name = updatedName;
    return Result.Success();
}

// Tomorrow: add business rule (no refactoring needed!)
public Result UpdateName(CatName updatedName)
{
    ArgumentNullException.ThrowIfNull(updatedName);

    if (Status == CatStatusType.Claimed)
        return Result.Failure(DomainErrors.Cat.CannotUpdateClaimed);

    Name = updatedName;
    return Result.Success();
}
```

**Trade-off:**

- Code duplication (13 similar methods)
- But: Consistency + future-proofing > DRY

**Alternative considered:** `internal set` - Rejected because:

- Mixed API (some properties have methods, some setters)
- Refactoring cost when adding validation
- Less explicit mutation points

### 3. ArgumentNullException for Input Validation

**Decision:** Every domain method validates null inputs with `ArgumentNullException.ThrowIfNull()`.

```csharp
public Result UpdateName(CatName updatedName)
{
    ArgumentNullException.ThrowIfNull(updatedName);  // ✅ Always present
    Name = updatedName;
    return Result.Success();
}
```

**Why:**

- Fail fast - Catch bugs at entry point, not deep in call stack
- 400 vs 500 - API returns Bad Request (400) not Internal Server Error (500)
- Clear contract - Method signature says "non-null required"
- Safety net - Tests catch regressions if someone removes the check

**Note on Nullable Reference Types:**

- NRT helps within C# code (compile-time)
- NRT does NOT help at system boundaries (HTTP/JSON → runtime data)
- ANE is required for API input validation

```csharp
// NRT doesn't help here:
[HttpPost]
public IActionResult UpdateCat([FromBody] UpdateCatRequest request)
{
    // request.Name can be null from JSON - NRT doesn't see this!
    Result<CatName> nameResult = CatName.Create(request.Name);
    // ^ ANE.ThrowIfNull needed inside Create()
}
```

**Trade-off:**

- Boilerplate in every method
- But: Explicit > implicit, safety > brevity

### 4. Test Every Null Check

**Decision:** We write tests for every `ArgumentNullException.ThrowIfNull()` call.

```csharp
[Fact]
public void UpdateName_ShouldThrow_WhenNullNameIsProvided()
{
    Cat cat = CatFactory.CreateRandom(Faker);

    Action updateName = () => cat.UpdateName(null!);

    updateName.ShouldThrow<ArgumentNullException>()
        .ParamName?.ToLower().ShouldContain("name");
}
```

**Why:**

- Regression safety - Catch if someone removes the null check
- API contract - Ensure 400 response, not 500
- Low maintenance cost - Simple tests, rarely change
- Explicit expectations - Tests document required behavior

**Common criticism:** "You're testing the framework"
**Response:** We're testing our usage of the framework. If we forget ANE, test fails.

**Trade-off:**

- Many "trivial" tests (2 per Update method = 26 tests in Cat alone)
- But: Safety net worth the cost

**Future improvement:**

- When NRT is fully enabled + strict mode
- Can remove tests for internal null checks (not API boundary tests)
- Compiler enforces non-null in domain

---

## 🎯 Domain Service Design

### 5. Accept Duplication in Services

**Decision:** `PersonUpdateService.UpdateEmailAsync()` and `UpdatePhoneNumberAsync()` have duplicated structure - this is intentional.

```csharp
// Similar code in both methods:
public async Task<Result> UpdateEmailAsync(PersonId personId, Email updatedEmail, ...)
{
    Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(...);
    if (maybePerson.HasNoValue)
        return Result.Failure(...);

    if (updatedEmail != maybePerson.Value.Email
        && await _uniquenessChecker.IsEmailTakenAsync(...))
        return Result.Failure(...);

    return maybePerson.Value.UpdateEmail(updatedEmail);
}

// UpdatePhoneNumberAsync has same structure
```

**Why duplication is better:**

- Readable - All logic visible in one place
- Debuggable - No callback/delegate maze
- Maintainable - Each method is independent
- Straightforward - Junior dev can understand immediately

**Alternative considered:** Generic method with delegates - Rejected:

```csharp
// ❌ REJECTED - Too clever
private async Task<Result> UpdateUniqueFieldAsync<T>(
    PersonId personId,
    T newValue,
    T currentValue,
    Func<T, CancellationToken, Task<bool>> isTakenChecker,  // Delegate hell
    Func<Person, T, Result> updater,
    Func<T, Error> alreadyTakenError,
    CancellationToken cancellationToken) where T : ValueObject
{
    // ... 10 lines using 5 delegates
}
```

**Why rejected:**

- Unreadable - 5 delegate parameters
- Hard to debug - Call stack full of lambdas
- Over-engineered - Solving duplication with complexity

**Rule of thumb:**

- 2 duplicates = OK
- 3-4 duplicates = Consider abstraction
- 5+ duplicates = Abstract

We have 2 methods → duplication is fine.

---

## 🧱 Value Objects

### 6. InfectiousDiseaseStatus - Critical Business Logic

**Decision:** `IsCompatibleWith()` logic is THE core business rule for cat safety.

```csharp
public bool IsCompatibleWith(InfectiousDiseaseStatus other)
{
    bool fivCompatible = FivStatus == other.FivStatus
                         || FivStatus is FivStatus.NotTested
                         || other.FivStatus is FivStatus.NotTested;

    bool felvCompatible = FelvStatus == other.FelvStatus
                          || FelvStatus is FelvStatus.NotTested
                          || other.FelvStatus is FelvStatus.NotTested;

    return fivCompatible && felvCompatible;
}
```

**Business rules:**

- FIV+ can only mix with FIV+ or NotTested
- FeLV+ can only mix with FeLV+ or NotTested
- NotTested can mix with anything (unknown status)
- Both conditions must be true

**Why this is well-designed:**

- Encapsulated in Value Object
- Clear business logic
- Well-tested (21 test cases cover all combinations)
- Used by Domain Services for validation

This is NOT duplication or over-engineering - this is core domain logic.

---

## 🚫 What We DON'T Do

Anti-patterns we avoid:

**❌ Internal setters for "simplicity"**

- Breaks future-proofing
- Inconsistent API (mix methods/setters)

**❌ Throwing exceptions from domain**

- Hidden control flow
- Hard to reason about error cases

**❌ Clever generics to avoid duplication**

- Readability > DRY
- Simple code > clever code

**❌ Skipping null checks "because NRT"**

- NRT doesn't work at system boundaries
- Better safe than sorry

---

## 📊 Metrics

| Metric | Value |
|--------|-------|
| Domain code | 4,649 lines (83 files) |
| Unit tests | 5,754 lines (27 files, 283 tests) |
| Test/Code ratio | 123% |
| Update* methods | ~24 methods across aggregates |
| Null check tests | ~56 tests (2 per Update method) |
| Integration tests | TODO (current gap) |

---

## 🔮 Future Improvements

When we grow beyond MVP:

**1. Enable strict Nullable Reference Types**
- Remove internal null check tests (keep API boundary tests)
- Compiler enforces non-null in domain

**2. Add integration tests**
- Test full flows: Create Cat → Announcement → Assign
- Test disease compatibility in real scenarios

**3. Consider Event Sourcing**
- If audit trail becomes critical
- Domain Events are already in place

**4. Performance optimization**
- Only if proven bottleneck
- Premature optimization is root of evil

---

## 💭 Philosophy

- **"Consistency > DRY"**
- **"Future-proofing > Current simplicity"**
- **"Readable > Clever"**
- **"Pragmatic > Academic"**

This codebase prioritizes:

- Long-term maintainability over short-term "clean code" metrics
- Predictable patterns over clever abstractions
- Explicit code over magic
- Safety nets (tests) over trust

---

## 🙋 Questions?

If you're reading this and think "why didn't they just X?":

- We probably considered it
- Check this doc for reasoning
- If not documented, ask the team

Remember: Every "obvious" solution has trade-offs. Our decisions are deliberate.
