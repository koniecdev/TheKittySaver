# Setup Azure Container Apps - krok po kroku

## Flow z Azure

```
git push
    │
    ▼
┌────────────────────────────────┐
│  GitHub Actions: Build & Test  │
└────────────────────────────────┘
    │
    ▼
┌────────────────────────────────┐
│  docker build & push           │
│  → Azure Container Registry    │
└────────────────────────────────┘
    │
    ▼
┌────────────────────────────────┐
│  Azure Container Apps          │
│  (automatycznie pobiera obraz) │
└────────────────────────────────┘
    │
    ▼
🌐 https://adoption-api.azurecontainerapps.io
```

## Różnica: VPS vs Azure Container Apps

| | VPS + Docker | Azure Container Apps |
|---|---|---|
| Serwer | Sam zarządzasz | Azure zarządza |
| Skalowanie | Ręczne | Automatyczne (0 → N instancji) |
| SSL | Sam konfigurujesz (Traefik) | Automatyczne |
| Koszt | Stały ($5-20/mies) | Pay-as-you-go (może być $0 jak nie ma ruchu) |
| docker-compose | Potrzebny na serwerze | Nie potrzebny |

---

## Jednorazowy setup w Azure (CLI)

```bash
# 1. Zaloguj się do Azure
az login

# 2. Utwórz Resource Group (kontener na wszystkie zasoby)
az group create \
  --name thekittysaver-rg \
  --location westeurope

# 3. Utwórz Azure Container Registry (jak Docker Hub, ale Azure)
az acr create \
  --name thekittysaver \
  --resource-group thekittysaver-rg \
  --sku Basic \
  --admin-enabled true

# 4. Pobierz credentials do ACR (do GitHub Secrets)
az acr credential show --name thekittysaver
# Zapisz username i password → GitHub Secrets

# 5. Utwórz Container Apps Environment
az containerapp env create \
  --name thekittysaver-env \
  --resource-group thekittysaver-rg \
  --location westeurope

# 6. Utwórz Container Apps (jedna per serwis)
az containerapp create \
  --name adoption-api \
  --resource-group thekittysaver-rg \
  --environment thekittysaver-env \
  --image thekittysaver.azurecr.io/adoption-api:latest \
  --target-port 8080 \
  --ingress external \
  --registry-server thekittysaver.azurecr.io \
  --registry-username <ACR_USERNAME> \
  --registry-password <ACR_PASSWORD>

# Powtórz dla auth-api i frontend
```

---

## GitHub Secrets do skonfigurowania

W repo → Settings → Secrets → Actions:

| Secret | Skąd wziąć |
|--------|-----------|
| `ACR_USERNAME` | `az acr credential show --name thekittysaver` |
| `ACR_PASSWORD` | j.w. |
| `AZURE_CREDENTIALS` | Service Principal (poniżej) |

### Tworzenie Service Principal (dla AZURE_CREDENTIALS)

```bash
az ad sp create-for-rbac \
  --name "github-actions-thekittysaver" \
  --role contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/thekittysaver-rg \
  --sdk-auth

# Output to JSON - skopiuj całość do GitHub Secrets jako AZURE_CREDENTIALS
```

---

## Co robi Azure Container Apps za Ciebie

1. **Ingress** - automatyczny HTTPS, routing, load balancing
2. **Scaling** - 0-N instancji na podstawie ruchu (możesz ustawić reguły)
3. **Revisions** - każdy deploy to nowa "rewizja", łatwy rollback
4. **Secrets** - wbudowane zarządzanie secretami (connection strings, API keys)
5. **Health checks** - automatyczne restartowanie niezdrowych kontenerów

---

## Koszty (orientacyjne, 2024)

| Zasób | Koszt |
|-------|-------|
| Container Registry (Basic) | ~$5/mies |
| Container Apps | ~$0.000024/vCPU-sekunda |
| | Mały ruch = kilka $ / mies |
| | Brak ruchu = ~$0 (scale to zero) |

**Dla side projectu z małym ruchem: $5-15/mies**

---

## Porównanie: GHCR vs ACR

| | GitHub Container Registry | Azure Container Registry |
|---|---|---|
| Darmowe | 500MB public, 2GB private | Brak darmowego |
| Koszt | Wliczone w GitHub | ~$5/mies (Basic) |
| Integracja | GitHub Actions | Azure Container Apps |
| Kiedy użyć | VPS, Railway, ogólnie | Azure hosting |

**Możesz użyć GHCR z Azure Container Apps**, ale ACR jest lepiej zintegrowane (szybszy pull, ten sam region).

---

## Alternatywa: Bez Dockera w CI (Azure buduje)

Azure Container Apps potrafi też budować z kodu źródłowego:

```bash
az containerapp up \
  --name adoption-api \
  --resource-group thekittysaver-rg \
  --source ./src/AdoptionSystem/TheKittySaver.AdoptionSystem.API
```

Wtedy nie potrzebujesz budować obrazów w GitHub Actions - Azure robi to za Ciebie.
Ale tracisz kontrolę i powtarzalność.
