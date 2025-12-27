# Plan: System Explicit Archiwizacji

## Cel
Zamiana implicit soft delete na explicit archiwizację. DELETE zostaje jako HARD DELETE tylko dla admina (te same ścieżki, inna autoryzacja).

---

## Model docelowy

```
┌─────────────────────────────────────────────────────────────┐
│  USER (frontend) - autoryzacja: owner zasobu                │
├─────────────────────────────────────────────────────────────┤
│  POST /cats/{id}/archive           → archiwizuje kota       │
│  POST /cats/{id}/unarchive         → przywraca kota         │
│                                                             │
│  POST /cats/{id}/vaccinations/{id}/archive   → archiwizuje  │
│  POST /cats/{id}/vaccinations/{id}/unarchive → przywraca    │
│                                                             │
│  POST /persons/{id}/archive        → archiwizuje + RODO     │
│  POST /persons/unarchive           → przywraca (już jest)   │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  ADMIN - autoryzacja: rola admin                            │
├─────────────────────────────────────────────────────────────┤
│  DELETE /cats/{id}                 → HARD DELETE            │
│  DELETE /cats/{id}/vaccinations/{id} → HARD DELETE          │
│  DELETE /persons/{id}              → HARD DELETE + CASCADE  │
│                                                             │
│  (te same ścieżki co były, tylko autoryzacja admin)        │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  USER - bez zmian (HARD DELETE zostaje)                     │
├─────────────────────────────────────────────────────────────┤
│  DELETE /cats/{id}/gallery/{id}    → HARD DELETE (pliki)    │
│  DELETE /cats/{id}/thumbnail       → HARD DELETE (pliki)    │
│  DELETE /adoption-announcements/{id} → HARD DELETE          │
│  DELETE /persons/{id}/addresses/{id} → HARD DELETE          │
└─────────────────────────────────────────────────────────────┘
```

---

## Faza 1: Domain - Vaccination IArchivable

### 1.1 Vaccination entity
- Dodać implementację `IArchivable` do `Vaccination`
- Dodać property `ArchivedAt`
- Dodać metody `Archive()` i `Unarchive()` (wzorować się na `Cat`)

### 1.2 Cat aggregate
- Dodać metody `ArchiveVaccination()` i `UnarchiveVaccination()`
- Wzorować się na istniejących metodach `RemoveVaccination()`, `UpdateVaccination...()`

### 1.3 Błędy domenowe
- Dodać `VaccinationEntity.IsArchived()` i `VaccinationEntity.IsNotArchived()`
- Wzorować się na `CatEntity.IsArchived()`

---

## Faza 2: EF Core - Vaccination Configuration

### 2.1 VaccinationConfiguration
- Dodać mapowanie `ArchivedAt` property
- Wzorować się na `CatConfiguration` lub `PersonConfiguration`

### 2.2 Query Filter (opcjonalnie)
- Rozważyć czy dodać `HasQueryFilter(x => x.ArchivedAt == null)`
- Rekomendacja: TAK - spójność z Cat i Person

---

## Faza 3: Migracja EF Core

- Wygenerować migrację dla nowej kolumny `ArchivedAt` w tabeli `Vaccinations`
- Zweryfikować że kolumna jest nullable

---

## Faza 4: Repository Layer

### 4.1 ICatRepository
- Dodać metodę do pobierania kota z IgnoreQueryFilters (do unarchive)
- Wzorować się na istniejących metodach `GetArchivedCatsByPersonIdAsync`

---

## Faza 5: API - Nowe endpointy Archive/Unarchive

### 5.1 Cat
- Nowy endpoint `POST /cats/{id}/archive`
- Nowy endpoint `POST /cats/{id}/unarchive`

### 5.2 Vaccination
- Nowy endpoint `POST /cats/{id}/vaccinations/{id}/archive`
- Nowy endpoint `POST /cats/{id}/vaccinations/{id}/unarchive`

### 5.3 Person
- Nowy endpoint `POST /persons/{id}/archive` (przenieść logikę z obecnego DELETE)
- Poprawić literówkę `UnarchievePerson` → `UnarchivePerson`

---

## Faza 6: Modyfikacja istniejących DELETE

### 6.1 Zmiana autoryzacji na admin-only
- `DELETE /cats/{id}` - dodać wymaganie roli admin
- `DELETE /cats/{id}/vaccinations/{id}` - dodać wymaganie roli admin
- `DELETE /persons/{id}` - dodać wymaganie roli admin

### 6.2 Bez zmian (user nadal ma dostęp)
- `DELETE /cats/{id}/gallery/{id}` - zostaje dla usera
- `DELETE /cats/{id}/thumbnail` - zostaje dla usera
- `DELETE /adoption-announcements/{id}` - zostaje dla usera
- `DELETE /persons/{id}/addresses/{id}` - zostaje dla usera

---

## Faza 7: Query Filters w Read Models

### 7.1 CatReadModelConfiguration
- Dodać `HasQueryFilter` dla `ArchivedAt`

### 7.2 AdoptionAnnouncementReadModelConfiguration
- Dodać `HasQueryFilter` dla `ArchivedAt`

---

## Faza 8: Testy

### 8.1 Domain
- Testy `Vaccination.Archive()` / `Unarchive()`
- Testy `Cat.ArchiveVaccination()` / `UnarchiveVaccination()`

### 8.2 API
- Testy nowych endpointów archive/unarchive
- Testy że DELETE wymaga roli admin
- Testy Query Filters (zasób nie widoczny po archiwizacji)

---

## Kolejność implementacji

```
[ ] Faza 1: Domain - Vaccination IArchivable
[ ] Faza 2: EF Configuration - Vaccination
[ ] Faza 3: Migracja EF Core
[ ] Faza 4: Repository - metody z IgnoreQueryFilters
[ ] Faza 5: API - nowe archive/unarchive endpoints
[ ] Faza 6: API - zmiana autoryzacji DELETE na admin
[ ] Faza 7: Query Filters w Read Models
[ ] Faza 8: Testy
```

---

## Pytania do rozstrzygnięcia

1. **Vaccination Query Filter** - filtrować automatycznie zarchiwizowane szczepienia?
2. **Literówka `UnarchievePerson`** - poprawić na `UnarchivePerson`?
3. **Admin DELETE** - implementować teraz czy zostawić na później?
