# 🐱 TheKittySaver

> System zarządzania adopcją kotów - MVP platformy do tworzenia ogłoszeń adopcyjnych, zarządzania profilami kotów i procesem adopcji.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-239120?style=flat-square&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-purple?style=flat-square)](https://docs.microsoft.com/en-us/ef/core/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue?style=flat-square)]()
[![Tests](https://img.shields.io/badge/Tests-283+-green?style=flat-square)]()

## 📋 Spis treści

- [O projekcie](#-o-projekcie)
- [Funkcjonalności](#-funkcjonalności)
- [Technologie](#-technologie)
- [Architektura](#-architektura)
- [Struktura projektu](#-struktura-projektu)
- [Uruchomienie](#-uruchomienie)
- [API Endpoints](#-api-endpoints)
- [Testy](#-testy)
- [Dokumentacja](#-dokumentacja)

## 🎯 O projekcie

**TheKittySaver** to backend API dla platformy adopcyjnej kotów, zaprojektowany zgodnie z zasadami **Clean Architecture**, **Domain-Driven Design (DDD)** oraz wzorcem **CQRS**.

Projekt powstał jako MVP (Minimum Viable Product) demonstrując najlepsze praktyki w budowaniu skalowalnych aplikacji .NET:
- Result Pattern zamiast wyjątków
- Strongly Typed IDs
- Value Objects z enkapsulacją logiki walidacji
- Domain Events
- Wysoki współczynnik pokrycia testami (123% test-to-code ratio)

## ✨ Funkcjonalności

### 🐈 Zarządzanie kotami
- Tworzenie i edycja profili kotów
- Szczegółowe informacje: wiek, płeć, kolor, waga, temperament
- Historia zdrowotna i status szczepień
- Status chorób zakaźnych (FIV/FeLV)
- Galeria zdjęć (do 20 zdjęć z miniaturką)
- Historia szczepień

### 👤 Zarządzanie osobami
- Rejestracja użytkowników
- Walidacja email i numeru telefonu (z libphonenumber)
- Obsługa wielu adresów (walidacja polskich kodów pocztowych)

### 📢 Ogłoszenia adopcyjne
- Tworzenie i publikacja ogłoszeń
- Przypisywanie kotów do ogłoszeń
- Reasygacja kotów między ogłoszeniami
- System roszczeń (claiming)
- Historia zmian (merge log)

## 🛠 Technologie

| Kategoria | Technologia |
|-----------|-------------|
| **Framework** | ASP.NET Core 10.0 |
| **Język** | C# 13 |
| **ORM** | Entity Framework Core 10.0 |
| **Baza danych** | SQL Server |
| **CQRS** | Mediator (Source Generated) |
| **Walidacja** | FluentValidation 12.1 |
| **API Versioning** | Asp.Versioning.Http 8.1 |
| **Testy** | xUnit, Shouldly, Bogus, Testcontainers |
| **Analiza kodu** | SonarAnalyzer |

## 🏗 Architektura

Projekt implementuje **Clean Architecture** z elementami **CQRS** i **DDD**:

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                            │
│         (Endpoints, Request/Response DTOs)              │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│                  Application Layer                      │
│            (Commands, Queries, Handlers)                │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│                   Domain Layer                          │
│     (Aggregates, Entities, Value Objects, Events)       │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│               Infrastructure Layer                      │
│       (DbContext, Repositories, External Services)      │
└─────────────────────────────────────────────────────────┘
```

### Główne agregaty domenowe

- **Cat** - Profil kota z galerią zdjęć i historią szczepień
- **Person** - Użytkownik systemu z adresami
- **AdoptionAnnouncement** - Ogłoszenie adopcyjne

### Wzorce projektowe

- **Result Pattern** - Przewidywalna obsługa błędów bez wyjątków
- **Strongly Typed IDs** - Bezpieczeństwo typów dla identyfikatorów
- **Value Objects** - Enkapsulacja logiki walidacji
- **Domain Events** - Luźne powiązania między komponentami
- **Mediator Pattern** - Odsprzęgnięcie handlerów od endpointów

## 📁 Struktura projektu

```
TheKittySaver/
├── src/AdoptionSystem/
│   ├── TheKittySaver.AdoptionSystem.API              # Web API, Endpoints
│   ├── TheKittySaver.AdoptionSystem.Domain           # Logika biznesowa
│   ├── TheKittySaver.AdoptionSystem.Domain.EntityFramework
│   ├── TheKittySaver.AdoptionSystem.Persistence      # DbContext, Migracje
│   ├── TheKittySaver.AdoptionSystem.Infrastructure   # Implementacje
│   ├── TheKittySaver.AdoptionSystem.Contracts        # DTOs
│   ├── TheKittySaver.AdoptionSystem.Primitives       # Strongly Typed IDs
│   ├── TheKittySaver.AdoptionSystem.ReadModels       # CQRS Read Models
│   └── TheKittySaver.AdoptionSystem.Calculators      # Logika obliczeniowa
│
├── tests/
│   ├── TheKittySaver.AdoptionSystem.Domain.Tests.Unit
│   └── TheKittySaver.AdoptionSystem.API.Tests.Integration
│
└── docs/
    ├── ARCHITECTURE.md
    ├── DOMAIN.md
    └── DomenaCatMedia_Flow.md
```

## 🚀 Uruchomienie

### Wymagania

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (lub SQL Server Express)
- [Docker](https://www.docker.com/) (opcjonalnie, dla testów integracyjnych)

### Konfiguracja bazy danych

1. Zaktualizuj connection string w `appsettings.json`:

```json
{
  "ConnectionStringSettings": {
    "Database": "Server=localhost\\SQLEXPRESS;Database=TheKittySaver;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

2. Wykonaj migracje:

```bash
cd src/AdoptionSystem/TheKittySaver.AdoptionSystem.Persistence
dotnet ef database update
```

### Uruchomienie API

```bash
cd src/AdoptionSystem/TheKittySaver.AdoptionSystem.API
dotnet run
```

API będzie dostępne pod adresami:
- HTTP: `http://localhost:5024`
- HTTPS: `https://localhost:7157`

### Docker

```bash
docker compose up
```

## 📡 API Endpoints

Wszystkie endpointy są wersjonowane (`/api/v1/...`).

### Koty (`/api/v1/cats`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| `POST` | `/api/v1/cats` | Tworzenie nowego kota |
| `GET` | `/api/v1/cats` | Lista kotów |
| `GET` | `/api/v1/cats/{id}` | Pobranie kota |
| `PUT` | `/api/v1/cats/{id}` | Aktualizacja kota |
| `DELETE` | `/api/v1/cats/{id}` | Usunięcie kota |
| `POST` | `/api/v1/cats/{id}/assign` | Przypisanie do ogłoszenia |
| `POST` | `/api/v1/cats/{id}/claim` | Roszczenie na kota |

### Galeria (`/api/v1/cats/{catId}/gallery`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| `POST` | `/api/v1/cats/{catId}/gallery` | Dodanie zdjęcia |
| `GET` | `/api/v1/cats/{catId}/gallery` | Lista zdjęć |
| `DELETE` | `/api/v1/cats/{catId}/gallery/{itemId}` | Usunięcie zdjęcia |
| `POST` | `/api/v1/cats/{catId}/gallery/reorder` | Zmiana kolejności |
| `POST` | `/api/v1/cats/{catId}/thumbnail` | Ustawienie miniatury |

### Szczepienia (`/api/v1/cats/{catId}/vaccinations`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| `POST` | `/api/v1/cats/{catId}/vaccinations` | Rejestracja szczepienia |
| `GET` | `/api/v1/cats/{catId}/vaccinations` | Historia szczepień |
| `PUT` | `/api/v1/cats/{catId}/vaccinations/{id}` | Aktualizacja |
| `DELETE` | `/api/v1/cats/{catId}/vaccinations/{id}` | Usunięcie |

### Osoby (`/api/v1/persons`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| `POST` | `/api/v1/persons` | Rejestracja osoby |
| `GET` | `/api/v1/persons` | Lista osób |
| `GET` | `/api/v1/persons/{id}` | Pobranie osoby |
| `PUT` | `/api/v1/persons/{id}` | Aktualizacja |
| `DELETE` | `/api/v1/persons/{id}` | Usunięcie |

### Adresy (`/api/v1/persons/{personId}/addresses`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| `POST` | `/api/v1/persons/{personId}/addresses` | Dodanie adresu |
| `GET` | `/api/v1/persons/{personId}/addresses` | Lista adresów |
| `PUT` | `/api/v1/persons/{personId}/addresses/{id}` | Aktualizacja |
| `DELETE` | `/api/v1/persons/{personId}/addresses/{id}` | Usunięcie |

### Ogłoszenia adopcyjne (`/api/v1/adoption-announcements`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| `POST` | `/api/v1/adoption-announcements` | Tworzenie ogłoszenia |
| `GET` | `/api/v1/adoption-announcements` | Lista ogłoszeń |
| `GET` | `/api/v1/adoption-announcements/{id}` | Pobranie ogłoszenia |
| `PUT` | `/api/v1/adoption-announcements/{id}` | Aktualizacja |
| `DELETE` | `/api/v1/adoption-announcements/{id}` | Usunięcie |
| `POST` | `/api/v1/adoption-announcements/{id}/claim` | Roszczenie |

## 🧪 Testy

Projekt posiada wysokie pokrycie testami (123% test-to-code ratio).

### Testy jednostkowe

```bash
dotnet test tests/TheKittySaver.AdoptionSystem.Domain.Tests.Unit
```

- **283+ testów** pokrywających logikę domenową
- Testy dla każdego agregatu, Value Object i Domain Service
- Testy null-check dla każdej metody z `ArgumentNullException.ThrowIfNull`

### Testy integracyjne

```bash
dotnet test tests/TheKittySaver.AdoptionSystem.API.Tests.Integration
```

- Wymagają **Docker** (Testcontainers dla SQL Server)
- Testują pełny przepływ HTTP -> Handler -> Database

### Uruchomienie wszystkich testów

```bash
dotnet test
```

## 📚 Dokumentacja

- [ARCHITECTURE.md](./ARCHITECTURE.md) - Decyzje architektoniczne i trade-offs
- [docs/DOMAIN.md](./docs/DOMAIN.md) - Dokumentacja domeny biznesowej
- [docs/DomenaCatMedia_Flow.md](./docs/DomenaCatMedia_Flow.md) - Diagramy przepływów

## 📊 Statystyki projektu

| Metryka | Wartość |
|---------|---------|
| Linie kodu domeny | ~4,649 |
| Linie testów | ~5,754 |
| Liczba testów | 283+ |
| Test/Code ratio | 123% |
| Endpointy API | 35+ |
| Agregaty domenowe | 3 |

---

<p align="center">
  <i>One does not simply save all the kitties.</i> 🐱
</p>
