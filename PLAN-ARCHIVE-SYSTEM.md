# Plan: System Explicit Archiwizacji

## Cel
Zamiana soft delete na explicit archiwizację z zachowaniem prawdziwego DELETE dla admina.

## Model docelowy

```
┌─────────────────────────────────────────────────────────────┐
│  USER (frontend)                                            │
├─────────────────────────────────────────────────────────────┤
│  POST /cats/{id}/archive           → archiwizuje kota       │
│  POST /cats/{id}/unarchive         → przywraca kota         │
│  POST /cats/{id}/vaccinations/{id}/archive   → archiwizuje  │
│  POST /cats/{id}/vaccinations/{id}/unarchive → przywraca    │
│                                                             │
│  POST /persons/{id}/archive        → archiwizuje + RODO     │
│  POST /persons/unarchive           → przywraca (już jest)   │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  ADMIN (backoffice / RODO / cleanup)                        │
├─────────────────────────────────────────────────────────────┤
│  DELETE /admin/cats/{id}           → HARD DELETE            │
│  DELETE /admin/persons/{id}        → HARD DELETE + CASCADE  │
│  DELETE /admin/vaccinations/{id}   → HARD DELETE            │
│                                                             │
│  (osobny kontroler z autoryzacją admin)                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Faza 1: Domain Layer - Vaccination IArchivable

### 1.1 Dodać IArchivable do CatVaccination
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/Entities/Vaccination.cs`

```csharp
public sealed class Vaccination : Entity<VaccinationId>, IArchivable
{
    public ArchivedAt? ArchivedAt { get; private set; }

    public Result Archive(ArchivedAt archivedAt) { ... }
    public Result Unarchive() { ... }
}
```

### 1.2 Dodać metody w Cat aggregate do archiwizacji vaccination
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/Entities/Cat.cs`

```csharp
public Result ArchiveVaccination(VaccinationId vaccinationId, ArchivedAt archivedAt) { ... }
public Result UnarchiveVaccination(VaccinationId vaccinationId) { ... }
```

### 1.3 Dodać błędy domenowe
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Core/Errors/DomainErrors.CatAggregate.cs`

```csharp
public static class VaccinationEntity
{
    public static Error IsArchived(VaccinationId id) => ...
    public static Error IsNotArchived(VaccinationId id) => ...
}
```

---

## Faza 2: EF Core Configuration

### 2.1 Konfiguracja Vaccination z ArchivedAt
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain.EntityFramework/Aggregates/CatAggregate/VaccinationConfiguration.cs`

```csharp
builder.Property(x => x.ArchivedAt)
    .HasConversion(
        v => v == null ? (DateTimeOffset?)null : v.Value,
        v => v == null ? null : ArchivedAt.CreateUnsafe(v.Value))
    .IsRequired(false);
```

### 2.2 Query Filter dla Vaccination (opcjonalnie - do dyskusji)
Czy chcemy ukrywać zarchiwizowane szczepienia automatycznie?
- TAK → dodać `HasQueryFilter(x => x.ArchivedAt == null)`
- NIE → szczepienia widoczne, ale oznaczone jako archived

**Rekomendacja:** TAK - spójność z Cat i Person

---

## Faza 3: API Layer - Nowe endpointy Archive/Unarchive

### 3.1 Cat Archive endpoints
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/Cats/ArchiveCat.cs`
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/Cats/UnarchiveCat.cs`

```
POST /cats/{catId}/archive
POST /cats/{catId}/unarchive
```

### 3.2 Vaccination Archive endpoints
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/CatsVaccinations/ArchiveCatVaccination.cs`
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/CatsVaccinations/UnarchiveCatVaccination.cs`

```
POST /cats/{catId}/vaccinations/{vaccinationId}/archive
POST /cats/{catId}/vaccinations/{vaccinationId}/unarchive
```

### 3.3 Person Archive endpoint (zamiana DELETE)
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/Persons/ArchivePerson.cs`
**Rename:** `UnarchievePerson.cs` → `UnarchivePerson.cs` (literówka w nazwie)

```
POST /persons/{personId}/archive    (nowy - logika z obecnego DELETE)
POST /persons/unarchive             (już istnieje - poprawić URL?)
```

**Uwaga:** Obecny `POST /persons/unarchive` nie ma `{personId}` - używa IdentityId z OIDC.
Do rozważenia czy ujednolicić.

---

## Faza 4: Usunięcie starych DELETE endpointów (user-facing)

### 4.1 Usunąć lub przenieść do admin:
- `DELETE /cats/{catId}` → usunąć (user nie ma dostępu)
- `DELETE /persons/{personId}` → usunąć (zastąpione przez POST archive)
- `DELETE /cats/{catId}/vaccinations/{vaccinationId}` → usunąć

### 4.2 Zachować (bez zmian):
- `DELETE /cats/{catId}/gallery/{galleryItemId}` → zostaje HARD DELETE (pliki)
- `DELETE /cats/{catId}/thumbnail` → zostaje HARD DELETE (pliki)
- `DELETE /adoption-announcements/{id}` → zostaje HARD DELETE (to tylko kontener)
- `DELETE /persons/{personId}/addresses/{addressId}` → zostaje HARD DELETE

---

## Faza 5: Admin Controller (opcjonalnie - do dyskusji)

### 5.1 Nowy kontroler admin
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/Admin/AdminDeleteCat.cs`
**Nowy plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/Features/Admin/AdminDeletePerson.cs`

```
DELETE /admin/cats/{catId}        → HARD DELETE (+ cascade gallery, thumbnail, vaccinations)
DELETE /admin/persons/{personId}  → HARD DELETE (+ cascade all)
```

**Autoryzacja:** Wymaga roli "Admin" lub specjalnego OIDC scope

### 5.2 Alternatywa: Bez admin DELETE na razie
- Archiwizowane dane zostają w bazie
- Cleanup przez SQL job / scheduled task
- Mniej kodu, mniej ryzyka

**Rekomendacja:** Na start bez admin DELETE - można dodać później

---

## Faza 6: Query Filters w Read Models

### 6.1 CatReadModel
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.ReadModels.EntityFramework/Aggregates/CatAggregate/CatReadModelConfiguration.cs`

```csharp
builder.HasQueryFilter(x => x.ArchivedAt == null);
```

### 6.2 AdoptionAnnouncementReadModel
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.ReadModels.EntityFramework/Aggregates/AdoptionAnnouncementAggregate/AdoptionAnnouncementReadModelConfiguration.cs`

```csharp
builder.HasQueryFilter(x => x.ArchivedAt == null);
```

---

## Faza 7: Migracja EF Core

### 7.1 Dodać ArchivedAt do Vaccination
```bash
cd src/AdoptionSystem/TheKittySaver.AdoptionSystem.Persistence
dotnet ef migrations add AddArchivedAtToVaccination
```

### 7.2 Zweryfikować migrację
- Kolumna `ArchivedAt` nullable w tabeli `Vaccinations`
- Brak breaking changes

---

## Faza 8: Repository Layer

### 8.1 ICatRepository - nowe metody
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/Repositories/ICatRepository.cs`

```csharp
Task<Maybe<Cat>> GetByIdIncludingArchivedAsync(CatId id, CancellationToken ct);
Task<IReadOnlyCollection<Cat>> GetArchivedCatsByPersonIdAsync(PersonId personId, CancellationToken ct);
```

### 8.2 CatRepository - implementacja z IgnoreQueryFilters
**Plik:** `src/AdoptionSystem/TheKittySaver.AdoptionSystem.Persistence/Repositories/CatRepository.cs`

```csharp
public async Task<Maybe<Cat>> GetByIdIncludingArchivedAsync(CatId id, CancellationToken ct)
{
    Cat? cat = await _dbContext.Cats
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(x => x.Id == id, ct);
    return cat ?? Maybe<Cat>.None;
}
```

---

## Faza 9: Testy

### 9.1 Testy jednostkowe Domain
- `Cat.Archive()` - happy path
- `Cat.Unarchive()` - happy path
- `Cat.Archive()` - już zarchiwizowany → błąd
- `Vaccination.Archive()` / `Unarchive()`

### 9.2 Testy integracyjne API
- `POST /cats/{id}/archive` → 204 No Content
- `POST /cats/{id}/unarchive` → 204 No Content
- `GET /cats/{id}` po archive → 404 (Query Filter)
- `POST /cats/{id}/archive` dla cudzego kota → 403

---

## Kolejność implementacji

```
[ ] Faza 1: Domain - Vaccination IArchivable
[ ] Faza 2: EF Configuration - Vaccination
[ ] Faza 7: Migracja (przed nowymi endpointami)
[ ] Faza 8: Repository methods
[ ] Faza 3: API - nowe archive/unarchive endpoints
[ ] Faza 4: Usunięcie starych DELETE
[ ] Faza 6: Query Filters w Read Models
[ ] Faza 9: Testy
[ ] Faza 5: Admin DELETE (opcjonalnie, później)
```

---

## Pytania do rozstrzygnięcia

1. **Vaccination Query Filter** - czy szczepienia też filtrować automatycznie?
2. **Admin DELETE** - implementować teraz czy później?
3. **UnarchivePerson URL** - `POST /persons/unarchive` vs `POST /persons/{id}/unarchive`?
4. **Literówka** - `UnarchievePerson` → `UnarchivePerson` (poprawić?)?

---

## Estymacja

| Faza | Złożoność |
|------|-----------|
| Faza 1-2 | Prosta |
| Faza 3 | Średnia (4 nowe pliki) |
| Faza 4 | Prosta (usuwanie kodu) |
| Faza 6-8 | Prosta |
| Faza 9 | Średnia |
| **Razem** | ~2-3h pracy |
