# CI/CD z Dockerem - Pełny Flow

## Diagram: Co się dzieje od commita do produkcji

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           DEVELOPER WORKFLOW                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   [1] git push origin main                                                  │
│          │                                                                   │
│          ▼                                                                   │
│   ┌──────────────────────────────────────────────────────────────────────┐  │
│   │                     GITHUB ACTIONS (CI)                               │  │
│   ├──────────────────────────────────────────────────────────────────────┤  │
│   │                                                                       │  │
│   │  [2] JOB: build-and-test                                             │  │
│   │      ├── checkout kodu                                                │  │
│   │      ├── dotnet restore                                               │  │
│   │      ├── dotnet build                                                 │  │
│   │      ├── dotnet test (unit)                                           │  │
│   │      └── dotnet test (integration)                                    │  │
│   │          │                                                            │  │
│   │          │ ✅ Testy przeszły                                          │  │
│   │          ▼                                                            │  │
│   │  [3] JOB: build-and-push-images (równolegle 3 obrazy!)               │  │
│   │      ├── docker login ghcr.io                                         │  │
│   │      ├── docker build -f Dockerfile.adoption-api                      │  │
│   │      ├── docker build -f Dockerfile.auth-api                          │  │
│   │      ├── docker build -f Dockerfile.frontend                          │  │
│   │      └── docker push ghcr.io/user/app:latest (×3)                     │  │
│   │          │                                                            │  │
│   │          ▼                                                            │  │
│   └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                         │
│                                    │ Obrazy w GHCR                          │
│                                    ▼                                         │
│   ┌──────────────────────────────────────────────────────────────────────┐  │
│   │                  GITHUB CONTAINER REGISTRY                            │  │
│   │                                                                       │  │
│   │   ghcr.io/koniecdev/thekittysaver/adoption-api:latest                │  │
│   │   ghcr.io/koniecdev/thekittysaver/adoption-api:abc123f               │  │
│   │   ghcr.io/koniecdev/thekittysaver/auth-api:latest                    │  │
│   │   ghcr.io/koniecdev/thekittysaver/frontend:latest                    │  │
│   │   ...                                                                 │  │
│   └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                         │
│                                    │ Trigger deploy                         │
│                                    ▼                                         │
│   ┌──────────────────────────────────────────────────────────────────────┐  │
│   │                     GITHUB ACTIONS (CD)                               │  │
│   ├──────────────────────────────────────────────────────────────────────┤  │
│   │                                                                       │  │
│   │  [4] JOB: deploy                                                      │  │
│   │      └── SSH do VPS:                                                  │  │
│   │          ├── cd /opt/thekittysaver                                    │  │
│   │          ├── docker compose pull     # pobierz nowe obrazy            │  │
│   │          ├── docker compose up -d    # zrestartuj kontenery          │  │
│   │          └── docker image prune -f   # posprzątaj                     │  │
│   │                                                                       │  │
│   └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                         │
│                                    ▼                                         │
│   ┌──────────────────────────────────────────────────────────────────────┐  │
│   │                        VPS (PRODUKCJA)                                │  │
│   ├──────────────────────────────────────────────────────────────────────┤  │
│   │                                                                       │  │
│   │  [5] Docker Compose uruchamia:                                        │  │
│   │                                                                       │  │
│   │     ┌─────────────┐      ┌─────────────┐      ┌─────────────┐        │  │
│   │     │   Traefik   │ ───▶ │ adoption-api│ ───▶ │  PostgreSQL │        │  │
│   │     │   (proxy)   │      └─────────────┘      └─────────────┘        │  │
│   │     │   :80/:443  │      ┌─────────────┐      ┌─────────────┐        │  │
│   │     │             │ ───▶ │   auth-api  │ ───▶ │    Redis    │        │  │
│   │     │   SSL auto  │      └─────────────┘      └─────────────┘        │  │
│   │     │             │      ┌─────────────┐                              │  │
│   │     │             │ ───▶ │  frontend   │                              │  │
│   │     └─────────────┘      └─────────────┘                              │  │
│   │                                                                       │  │
│   └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                         │
│                                    ▼                                         │
│                         🌐 https://thekittysaver.com                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Wyjaśnienie krok po kroku

### KROK 1: Developer pushuje kod

```bash
git add .
git commit -m "feat: add cat adoption feature"
git push origin main
```

### KROK 2: CI - Build & Test

GitHub Actions automatycznie:
- Pobiera kod
- Instaluje .NET
- Kompiluje projekt
- Uruchamia testy

**Jeśli testy FAILUJĄ** → Pipeline się zatrzymuje, obrazy NIE są budowane!

### KROK 3: CI - Build & Push Images

Po przejściu testów:
- Logowanie do GitHub Container Registry (GHCR)
- `docker build` dla każdego serwisu
- `docker push` do registry z tagami:
  - `latest` - najnowsza wersja
  - `abc123f` - SHA commita (do rollbacków!)

### KROK 4: CD - Deploy

GitHub Actions łączy się z VPS przez SSH i wykonuje:

```bash
docker compose pull      # Pobiera nowe obrazy z GHCR
docker compose up -d     # Restartuje kontenery z nowymi obrazami
```

### KROK 5: Aplikacja działa

Na VPS Docker Compose zarządza:
- **Traefik** - reverse proxy, SSL, routing
- **Serwisy** - Twoje API + Frontend
- **Bazy danych** - PostgreSQL, Redis

---

## Pytania rekrutacyjne i odpowiedzi

### Q: Dlaczego używamy Container Registry zamiast budować na serwerze?

**A:**
1. **Separacja odpowiedzialności** - CI buduje, serwer tylko uruchamia
2. **Szybszy deploy** - `docker pull` zajmuje sekundy vs minuty na build
3. **Rollback** - mamy historię obrazów, możemy wrócić do poprzedniej wersji
4. **Spójność** - ten sam obraz na staging i produkcji

### Q: Co to jest multi-stage build w Dockerfile?

**A:** Technika optymalizacji rozmiaru obrazu:
```dockerfile
# Stage 1: Build (duży obraz z SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN dotnet publish -o /app

# Stage 2: Runtime (mały obraz tylko z runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY --from=build /app .
```
Wynik: Obraz 200MB zamiast 2GB.

### Q: Jak zrobić zero-downtime deployment?

**A:** Docker Compose z health checks + Traefik:
1. Nowy kontener startuje
2. Health check sprawdza czy jest gotowy
3. Traefik zaczyna kierować ruch do nowego
4. Stary kontener jest zatrzymywany

### Q: Jak przechowywać sekrety (hasła, klucze)?

**A:**
- **GitHub Secrets** - dla CI/CD pipeline (`${{ secrets.JWT_SECRET }}`)
- **Plik .env** na serwerze - dla Docker Compose (NIE w repo!)
- **Vault/Key Vault** - dla enterprise (HashiCorp Vault, Azure Key Vault)

### Q: Co jeśli deploy się nie uda?

**A:** Rollback do poprzedniej wersji:
```bash
# Na VPS:
docker compose pull ghcr.io/user/app:abc123f   # Poprzedni SHA
docker compose up -d
```
Dlatego tagujemy obrazy SHA commita, nie tylko `latest`.

### Q: Czym różni się CI od CD?

**A:**
- **CI (Continuous Integration)** - automatyczne budowanie i testowanie przy każdym PUSHU
- **CD (Continuous Delivery)** - automatyczne przygotowanie do deployu (obrazy w registry)
- **CD (Continuous Deployment)** - automatyczny deploy na produkcję

---

## Komendy do zapamiętania

```bash
# Lokalne testowanie docker-compose
docker compose -f docker-compose.prod.yml config    # Walidacja
docker compose -f docker-compose.prod.yml up -d     # Start
docker compose logs -f adoption-api                  # Logi
docker compose ps                                    # Status

# Na serwerze - manual deploy
ssh user@vps
cd /opt/thekittysaver
docker compose pull && docker compose up -d

# Rollback
docker compose pull ghcr.io/user/app:poprzedni-sha
docker compose up -d

# Debugowanie
docker compose exec adoption-api sh     # Shell w kontenerze
docker compose logs --tail=100 -f       # Ostatnie 100 linii logów
```

---

## Checklist przed rozmową

- [ ] Umiem wyjaśnić różnicę między CI a CD
- [ ] Wiem po co jest Container Registry
- [ ] Rozumiem multi-stage build
- [ ] Potrafię wyjaśnić zero-downtime deployment
- [ ] Wiem jak przechowywać sekrety
- [ ] Umiem zrobić rollback
- [ ] Rozumiem rolę reverse proxy (Traefik/nginx)
