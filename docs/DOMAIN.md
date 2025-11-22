# Dokumentacja Domeny - TheKittySaver

## Spis treści

1. [Przegląd architektury](#przegląd-architektury)
2. [Agregaty](#agregaty)
3. [Obiekty wartości (Value Objects)](#obiekty-wartości-value-objects)
4. [Zdarzenia domenowe (Domain Events)](#zdarzenia-domenowe-domain-events)
5. [Serwisy domenowe](#serwisy-domenowe)
6. [Repozytoria](#repozytoria)
7. [Abstrakcje i wzorce](#abstrakcje-i-wzorce)
8. [Obsługa błędów](#obsługa-błędów)
9. [Struktura projektu](#struktura-projektu)

---

## Przegląd architektury

Projekt implementuje wzorzec **Domain-Driven Design (DDD)** z czystą separacją między logiką domenową a infrastrukturą.

### Kluczowe cechy architektury:

- **Agregaty** jako główne jednostki spójności biznesowej
- **Obiekty wartości** dla niezmiennych danych
- **Zdarzenia domenowe** do śledzenia zmian stanu
- **Wzorzec Result Monad** do jawnej obsługi błędów
- **Wzorzec Repository** do abstrakcji persystencji

---

## Agregaty

### 1. Agregat Cat (Kot)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/`

#### Korzeń agregatu: `Cat`

Reprezentuje kota w systemie adopcji.

| Właściwość | Typ | Opis |
|------------|-----|------|
| `Id` | `CatId` | Unikalny identyfikator kota |
| `PersonId` | `PersonId` | Referencja do właściciela |
| `AdoptionAnnouncementId` | `AdoptionAnnouncementId?` | Opcjonalna referencja do ogłoszenia adopcyjnego |
| `Status` | `CatStatusType` | Status kota (Draft, Published, Claimed) |
| `Name` | `CatName` | Imię kota |
| `Age` | `CatAge` | Wiek kota |
| `Gender` | `CatGender` | Płeć kota |
| `Color` | `CatColor` | Kolor sierści |
| `Weight` | `CatWeight` | Waga kota |
| `Description` | `CatDescription` | Opis kota |
| `HealthStatus` | `HealthStatus` | Status zdrowia |
| `SpecialNeedsStatus` | `SpecialNeedsStatus` | Status specjalnych potrzeb |
| `Temperament` | `Temperament` | Temperament kota |
| `AdoptionHistory` | `AdoptionHistory` | Historia adopcji |
| `ListingSource` | `ListingSource` | Źródło ogłoszenia |
| `NeuteringStatus` | `NeuteringStatus` | Status kastracji/sterylizacji |
| `InfectiousDiseaseStatus` | `InfectiousDiseaseStatus` | Status chorób zakaźnych (FIV/FeLV) |
| `CreatedAt` | `CreatedAt` | Data utworzenia |
| `ClaimedAt` | `ClaimedAt?` | Data adopcji |
| `PublishedAt` | `PublishedAt?` | Data publikacji |

#### Encja potomna: `Vaccination` (Szczepienie)

| Właściwość | Typ | Opis |
|------------|-----|------|
| `Id` | `VaccinationId` | Identyfikator szczepienia |
| `Type` | `VaccinationType` | Rodzaj szczepienia |
| `Dates` | `VaccinationDates` | Daty szczepienia |
| `VeterinarianNote` | `VaccinationNote?` | Notatka weterynarza |
| `CreatedAt` | `CreatedAt` | Data utworzenia |

#### Możliwości biznesowe:

- Przypisywanie/odpinanie od ogłoszeń adopcyjnych
- Przenoszenie między ogłoszeniami
- Oznaczanie jako zaadoptowany (Claim)
- Zarządzanie szczepieniami (dodawanie, usuwanie, aktualizacja)
- Aktualizacja właściwości kota

---

### 2. Agregat Person (Osoba)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/PersonAggregate/`

#### Korzeń agregatu: `Person`

Reprezentuje użytkownika systemu (właściciela kotów).

| Właściwość | Typ | Opis |
|------------|-----|------|
| `Id` | `PersonId` | Unikalny identyfikator osoby |
| `IdentityId` | `Guid?` | Referencja do systemu tożsamości |
| `Username` | `Username` | Nazwa użytkownika |
| `Email` | `Email` | Adres email |
| `PhoneNumber` | `PhoneNumber` | Numer telefonu |
| `CreatedAt` | `CreatedAt` | Data utworzenia |

#### Encja potomna: `Address` (Adres)

| Właściwość | Typ | Opis |
|------------|-----|------|
| `Id` | `AddressId` | Identyfikator adresu |
| `PersonId` | `PersonId` | Referencja do osoby |
| `CountryCode` | `Country` | Kod kraju |
| `Name` | `AddressName` | Nazwa adresu |
| `Region` | `AddressRegion` | Region/województwo |
| `City` | `AddressCity` | Miasto |
| `Line` | `AddressLine?` | Linia adresowa |
| `CreatedAt` | `CreatedAt` | Data utworzenia |

#### Możliwości biznesowe:

- Dodawanie/aktualizacja/usuwanie adresów
- Ustawianie identyfikatora tożsamości
- Aktualizacja danych kontaktowych

---

### 3. Agregat AdoptionAnnouncement (Ogłoszenie adopcyjne)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/AdoptionAnnouncementAggregate/`

#### Korzeń agregatu: `AdoptionAnnouncement`

Reprezentuje ogłoszenie adopcyjne, do którego mogą być przypisane koty.

| Właściwość | Typ | Opis |
|------------|-----|------|
| `Id` | `AdoptionAnnouncementId` | Unikalny identyfikator ogłoszenia |
| `PersonId` | `PersonId` | Referencja do twórcy ogłoszenia |
| `Status` | `AnnouncementStatusType` | Status ogłoszenia (Active, Claimed) |
| `Description` | `AdoptionAnnouncementDescription?` | Opis ogłoszenia |
| `Address` | `AdoptionAnnouncementAddress` | Adres odbioru |
| `Email` | `Email` | Email kontaktowy |
| `PhoneNumber` | `PhoneNumber` | Telefon kontaktowy |
| `ClaimedAt` | `ClaimedAt?` | Data zakończenia (adopcji) |
| `CreatedAt` | `CreatedAt` | Data utworzenia |

#### Obiekt wartości: `AdoptionAnnouncementMergeLog`

Śledzi historię scalonych ogłoszeń.

#### Możliwości biznesowe:

- Aktualizacja opisu, adresu, danych kontaktowych
- Oznaczanie jako zakończone (Claim)
- Śledzenie scalonych ogłoszeń

---

## Obiekty wartości (Value Objects)

### Obiekty wartości agregatu Cat

| Nazwa | Walidacja | Opis |
|-------|-----------|------|
| `CatName` | max 50 znaków | Imię kota |
| `CatDescription` | max 250 znaków | Opis kota |
| `CatAge` | 0-40 lat | Wiek kota |
| `CatGender` | enum: Male, Female | Płeć kota |
| `CatColor` | enum: Black, White, Orange, Gray, Tabby, Calico, Tortoiseshell, BlackAndWhite, Other | Kolor sierści |
| `CatWeight` | 0.5-20 kg | Waga kota |
| `HealthStatus` | enum: Healthy, MinorIssues, Recovering, ChronicIllness, Critical | Status zdrowia |
| `SpecialNeedsStatus` | bool + opis + poziom (None, Minor, Major, Critical) | Status specjalnych potrzeb |
| `Temperament` | enum: Friendly, Independent, Timid, VeryTimid, Aggressive | Temperament |
| `AdoptionHistory` | ReturnCount, LastReturnDate, LastReturnReason | Historia adopcji |
| `ListingSource` | enum + nazwa: PrivatePerson, PrivatePersonUrgent, Shelter, Foundation | Źródło ogłoszenia |
| `NeuteringStatus` | bool (IsNeutered) | Status kastracji |
| `InfectiousDiseaseStatus` | status FIV/FeLV + data testu | Status chorób zakaźnych |
| `VaccinationDates` | data + opcjonalna następna data | Daty szczepienia |
| `VaccinationNote` | max 250 znaków | Notatka do szczepienia |

### Obiekty wartości agregatu Person

| Nazwa | Walidacja | Opis |
|-------|-----------|------|
| `Username` | max 150 znaków | Nazwa użytkownika |
| `AddressName` | max 150 znaków | Nazwa adresu |

### Obiekty wartości agregatu AdoptionAnnouncement

| Nazwa | Walidacja | Opis |
|-------|-----------|------|
| `AdoptionAnnouncementDescription` | max 250 znaków | Opis ogłoszenia |
| `AdoptionAnnouncementAddress` | złożony (City, Region, Line?) | Adres odbioru |
| `AdoptionAnnouncementMergeLog` | referencje do ID | Log scaleń |

### Współdzielone obiekty wartości

| Nazwa | Walidacja | Opis |
|-------|-----------|------|
| `Email` | regex, max 250 znaków | Adres email |
| `PhoneNumber` | max 30 znaków, factory pattern | Numer telefonu |
| `AddressCity` | max 100 znaków | Miasto |
| `AddressRegion` | max 200 znaków | Region |
| `AddressLine` | max 500 znaków | Linia adresowa |
| `CreatedAt` | min: 2025-01-01 | Data utworzenia |
| `ClaimedAt` | min: 2025-01-01 | Data adopcji |
| `PublishedAt` | min: 2025-01-01 | Data publikacji |
| `ArchivedAt` | min: 2025-01-01 | Data archiwizacji |

---

## Zdarzenia domenowe (Domain Events)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/Events/`

### 1. CatClaimedDomainEvent

Wywoływane gdy kot zostaje zaadoptowany.

```csharp
public record CatClaimedDomainEvent(CatId CatId, AdoptionAnnouncementId AdoptionAnnouncementId) : DomainEvent;
```

### 2. CatReassignedToAnotherAnnouncementDomainEvent

Wywoływane gdy kot zostaje przeniesiony z jednego ogłoszenia do innego.

```csharp
public record CatReassignedToAnotherAnnouncementDomainEvent(
    CatId CatId,
    AdoptionAnnouncementId SourceAdoptionAnnouncementId,
    AdoptionAnnouncementId DestinationAdoptionAnnouncementId) : DomainEvent;
```

### 3. CatUnassignedFromAnnouncementDomainEvent

Wywoływane gdy kot zostaje odpięty od ogłoszenia (powrót do statusu Draft).

```csharp
public record CatUnassignedFromAnnouncementDomainEvent(
    CatId CatId,
    AdoptionAnnouncementId AdoptionAnnouncementId) : DomainEvent;
```

---

## Serwisy domenowe

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Services/`

### 1. CatAdoptionAnnouncementAssignmentService

Odpowiada za przypisywanie kotów do ogłoszeń adopcyjnych.

**Interfejs:** `ICatAdoptionAnnouncementAssignmentService`

**Walidacje:**
- Zgodność PersonId (właściciel kota = twórca ogłoszenia)
- Status kota = Draft
- Status ogłoszenia = Active
- Kot nie jest już przypisany do innego ogłoszenia
- Kompatybilność chorób zakaźnych między kotami w ogłoszeniu

### 2. AdoptionAnnouncementCreationService

Odpowiada za tworzenie nowych ogłoszeń adopcyjnych.

**Interfejs:** `IAdoptionAnnouncementCreationService`

**Funkcjonalność:**
- Tworzenie nowego ogłoszenia
- Atomowe przypisanie pierwszego kota do ogłoszenia

### 3. CatAdoptionAnnouncementReassignmentService

Odpowiada za przenoszenie kotów między ogłoszeniami.

**Interfejs:** `ICatAdoptionAnnouncementReassignmentService`

### 4. PersonCreationService

Odpowiada za tworzenie nowych użytkowników.

**Interfejs:** `IPersonCreationService`

**Walidacje (asynchroniczne):**
- Unikalność adresu email
- Unikalność numeru telefonu

**Zależności:** `IPersonUniquenessCheckerService`

### 5. PersonUpdateService

Odpowiada za aktualizację danych użytkownika.

### 6. IPersonUniquenessCheckerService

Serwis sprawdzający unikalność danych osoby.

---

## Repozytoria

**Wzorzec:** Generyczne repozytorium z rozszerzeniami dla agregatów

### Interfejs bazowy

```csharp
public interface IRepository<TAggregateRoot, in TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    Task<TAggregateRoot?> GetByIdAsync(TAggregateRootId id, CancellationToken ct = default);
    void Insert(TAggregateRoot entity);
    void Remove(TAggregateRoot entity);
}
```

### Specyficzne repozytoria

| Interfejs | Dodatkowe metody |
|-----------|------------------|
| `ICatRepository` | `GetCatsByAdoptionAnnouncementIdAsync()` |
| `IPersonRepository` | - |
| `IAdoptionAnnouncementRepository` | - |

---

## Abstrakcje i wzorce

### Klasy bazowe

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Core/BuildingBlocks/`

#### AggregateRoot<TId>

Klasa bazowa dla korzeni agregatów.

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
{
    protected void RaiseDomainEvent(IDomainEvent domainEvent);
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    public void ClearDomainEvents();
}
```

#### Entity<TId>

Klasa bazowa dla encji.

```csharp
public abstract class Entity<TId>
{
    public TId Id { get; }
    public CreatedAt CreatedAt { get; }
    // Równość oparta na ID
}
```

#### ValueObject

Klasa bazowa dla obiektów wartości.

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetAtomicValues();
    // Równość oparta na wartościach
}
```

### Interfejsy cross-cutting

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Core/Abstractions/`

| Interfejs | Właściwości | Metody | Implementacje |
|-----------|-------------|--------|---------------|
| `IClaimable` | `ClaimedAt` | `Claim(ClaimedAt)` | `Cat`, `AdoptionAnnouncement` |
| `IPublishable` | `PublishedAt` | - | `Cat` |
| `IArchivable` | `ArchivedAt` | - | - |

### Monady

#### Result Monad

Reprezentuje wynik operacji (sukces/porażka) bez użycia wyjątków.

```csharp
// Operacje bez wartości zwracanej
Result.Success();
Result.Failure(error);

// Operacje z wartością zwracaną
Result<TValue>.Success(value);
Result<TValue>.Failure(error);
```

#### Maybe Monad

Reprezentuje wartość opcjonalną.

---

## Obsługa błędów

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Core/Errors/`

### Kategorie błędów

| Klasa | Opis |
|-------|------|
| `BaseDomainErrors` | Wspólne błędy domenowe |
| `CatAggregateDomainErrors` | Błędy specyficzne dla agregatu Cat |
| `PersonAggregateDomainErrors` | Błędy specyficzne dla agregatu Person |
| `AdoptionAnnouncementAggregateDomainErrors` | Błędy specyficzne dla ogłoszeń |
| `SharedValueObjectsDomainErrors` | Błędy współdzielonych obiektów wartości |
| `CatAdoptionAnnouncementServiceDomainErrors` | Błędy serwisów domenowych |

### Struktura błędu

```csharp
public class Error
{
    public string Code { get; }
    public string Message { get; }
}
```

---

## Struktura projektu

```
/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/
├── Aggregates/
│   ├── CatAggregate/
│   │   ├── Entities/
│   │   │   ├── Cat.cs
│   │   │   └── Vaccination.cs
│   │   ├── ValueObjects/
│   │   │   ├── CatName.cs
│   │   │   ├── CatAge.cs
│   │   │   ├── CatGender.cs
│   │   │   ├── CatColor.cs
│   │   │   ├── CatWeight.cs
│   │   │   ├── CatDescription.cs
│   │   │   ├── HealthStatus.cs
│   │   │   ├── SpecialNeedsStatus.cs
│   │   │   ├── Temperament.cs
│   │   │   ├── AdoptionHistory.cs
│   │   │   ├── ListingSource.cs
│   │   │   ├── NeuteringStatus.cs
│   │   │   ├── InfectiousDiseaseStatus.cs
│   │   │   ├── VaccinationDates.cs
│   │   │   └── VaccinationNote.cs
│   │   ├── Events/
│   │   │   ├── CatClaimedDomainEvent.cs
│   │   │   ├── CatReassignedToAnotherAnnouncementDomainEvent.cs
│   │   │   └── CatUnassignedFromAnnouncementDomainEvent.cs
│   │   └── Repositories/
│   │       └── ICatRepository.cs
│   │
│   ├── PersonAggregate/
│   │   ├── Entities/
│   │   │   ├── Person.cs
│   │   │   └── Address.cs
│   │   ├── ValueObjects/
│   │   │   ├── Username.cs
│   │   │   └── AddressName.cs
│   │   ├── Services/
│   │   │   ├── PersonCreationService.cs
│   │   │   └── PersonUpdateService.cs
│   │   └── Repositories/
│   │       └── IPersonRepository.cs
│   │
│   └── AdoptionAnnouncementAggregate/
│       ├── Entities/
│       │   └── AdoptionAnnouncement.cs
│       ├── ValueObjects/
│       │   ├── AdoptionAnnouncementDescription.cs
│       │   ├── AdoptionAnnouncementAddress.cs
│       │   └── AdoptionAnnouncementMergeLog.cs
│       └── Repositories/
│           └── IAdoptionAnnouncementRepository.cs
│
├── Services/
│   ├── AdoptionAnnouncementCreationServices/
│   │   └── AdoptionAnnouncementCreationService.cs
│   ├── CatAdoptionAnnouncementServices/
│   │   └── CatAdoptionAnnouncementAssignmentService.cs
│   └── CatAdoptionAnnouncementReassignmentServices/
│       └── CatAdoptionAnnouncementReassignmentService.cs
│
├── SharedValueObjects/
│   ├── AddressCompounds/
│   │   ├── AddressCity.cs
│   │   ├── AddressRegion.cs
│   │   └── AddressLine.cs
│   ├── PhoneNumbers/
│   │   ├── PhoneNumber.cs
│   │   └── PhoneNumberFactory.cs
│   ├── Email.cs
│   └── Timestamps/
│       ├── CreatedAt.cs
│       ├── ClaimedAt.cs
│       ├── PublishedAt.cs
│       └── ArchivedAt.cs
│
└── Core/
    ├── BuildingBlocks/
    │   ├── AggregateRoot.cs
    │   ├── Entity.cs
    │   ├── ValueObject.cs
    │   ├── DomainEvent.cs
    │   ├── IDomainEvent.cs
    │   ├── Error.cs
    │   └── IRepository.cs
    ├── Abstractions/
    │   ├── IClaimable.cs
    │   ├── IPublishable.cs
    │   └── IArchivable.cs
    ├── Monads/
    │   ├── Result.cs
    │   └── Maybe.cs
    ├── Errors/
    │   ├── BaseDomainErrors.cs
    │   ├── CatAggregateDomainErrors.cs
    │   ├── PersonAggregateDomainErrors.cs
    │   ├── AdoptionAnnouncementAggregateDomainErrors.cs
    │   ├── SharedValueObjectsDomainErrors.cs
    │   └── CatAdoptionAnnouncementServiceDomainErrors.cs
    ├── Guards/
    ├── Enums/
    │   ├── CatStatusType.cs
    │   ├── AnnouncementStatusType.cs
    │   ├── VaccinationType.cs
    │   └── Country.cs
    └── Extensions/
```

---

## Relacje między encjami

```
┌──────────────────┐
│     Person       │
│   (Aggregate)    │
└────────┬─────────┘
         │
         │ 1:N
         ▼
┌──────────────────┐         ┌──────────────────────────┐
│     Address      │         │   AdoptionAnnouncement   │
│    (Entity)      │         │       (Aggregate)        │
└──────────────────┘         └────────────┬─────────────┘
                                          │
         ┌────────────────────────────────┤
         │                                │
         │ Person tworzy                  │ 0:N
         │ ogłoszenie (1:N)               │
         │                                ▼
         │                    ┌──────────────────┐
         │                    │       Cat        │
         │                    │   (Aggregate)    │
         │                    └────────┬─────────┘
         │                             │
         │ Person jest                 │ 1:N
         │ właścicielem (1:N)          ▼
         │                    ┌──────────────────┐
         └───────────────────►│   Vaccination    │
                              │    (Entity)      │
                              └──────────────────┘
```

---

## Statystyki

| Kategoria | Liczba |
|-----------|--------|
| Agregaty | 3 |
| Encje potomne | 2 |
| Obiekty wartości | 27+ |
| Zdarzenia domenowe | 3 |
| Serwisy domenowe | 6 |
| Interfejsy repozytoriów | 3 |
| Typy błędów domenowych | 6 |
| Abstrakcje cross-cutting | 3 |

---

## Wzorce projektowe

1. **Domain-Driven Design (DDD)** - główna architektura
2. **Aggregate Pattern** - grupowanie powiązanych encji
3. **Value Object Pattern** - niezmienne obiekty z semantyką wartości
4. **Repository Pattern** - abstrakcja persystencji
5. **Domain Event Pattern** - przechwytywanie zmian stanu domeny
6. **Domain Service Pattern** - operacje cross-aggregate
7. **Result Monad** - jawna obsługa błędów
8. **Factory Pattern** - złożone tworzenie obiektów (np. PhoneNumberFactory)
9. **Specification Pattern** - niejawnie w logice walidacji
