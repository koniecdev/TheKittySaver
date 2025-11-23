# Flow Domeny - System Zarządzania Mediami Kotów

## Spis treści

1. [Przegląd koncepcji](#przegląd-koncepcji)
2. [Struktura danych](#struktura-danych)
3. [Konwencje storage](#konwencje-storage)
4. [Flow biznesowe](#flow-biznesowe)
5. [Invarianty domenowe](#invarianty-domenowe)
6. [Przykłady użycia](#przykłady-użycia)

---

## Przegląd koncepcji

System zarządzania mediami kotów składa się z dwóch odrębnych mechanizmów:

1. **Miniaturka (Thumbnail)** - pojedyncze zdjęcie reprezentujące kota
2. **Galeria (Gallery)** - zbiór do 20 elementów (zdjęcia + filmy) z możliwością organizacji

### Kluczowe założenia:

- Miniaturka jest **Value Object** - niezmienne ID reprezentujące zdjęcie
- Element galerii jest **Encją** - ma tożsamość ze względu na modyfikowalny `DisplayOrder`
- Domena operuje tylko na identyfikatorach - fizyczne pliki zarządzane są przez infrastrukturę
- Storage oparty na konwencji nazewnictwa plików

---

## Struktura danych

### CatThumbnail (Value Object)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/ValueObjects/CatThumbnail.cs`

```csharp
public sealed class CatThumbnail : ValueObject
{
    public CatImageId CatImageId { get; }
}
```

| Właściwość | Typ | Opis |
|------------|-----|------|
| `CatImageId` | `CatImageId` | Unikalny identyfikator zdjęcia miniaturki |

**Dlaczego Value Object?**
- Niezmienne - raz utworzone ID nie ulega zmianie
- Brak tożsamości biznesowej - liczy się tylko wartość ID
- Wymiana miniaturki = stworzenie nowego `CatThumbnail`

---

### CatGalleryItem (Entity)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/Entities/CatGalleryItem.cs`

```csharp
public sealed class CatGalleryItem : Entity<CatGalleryItemId>
{
    public CatGalleryItemDisplayOrder DisplayOrder { get; private set; }
    
    internal void UpdateDisplayOrder(CatGalleryItemDisplayOrder order);
}
```

| Właściwość | Typ | Opis |
|------------|-----|------|
| `Id` | `CatGalleryItemId` | Unikalny identyfikator elementu galerii |
| `DisplayOrder` | `CatGalleryItemDisplayOrder` | Pozycja w galerii (0-19) |
| `CreatedAt` | `CreatedAt` | Data utworzenia |

**Dlaczego Entity?**
- Ma tożsamość biznesową - konkretny element może zmieniać pozycję
- Modyfikowalny `DisplayOrder` - drag & drop wymaga mutacji
- Cykl życia niezależny od wartości - element o ID=X zawsze pozostaje elementem o ID=X

---

### CatGalleryItemDisplayOrder (Value Object)

**Lokalizacja:** `/src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/Aggregates/CatAggregate/ValueObjects/CatGalleryItemDisplayOrder.cs`

```csharp
public sealed class CatGalleryItemDisplayOrder : ValueObject
{
    public const int MinValue = 0;
    public int Value { get; }
}
```

**Walidacja:**
- Minimum: `0`
- Maksimum: `MaximumGalleryItemsCount - 1` (aktualnie 19)

---

## Konwencje storage

### Struktura katalogów

```
src/pictures/cats/{CatId}/
├── thumbnail/
│   └── {CatImageId}.webp
└── gallery/
    ├── {CatGalleryItemId}.webp
    ├── {CatGalleryItemId}.webp
    └── {CatGalleryItemId}.mp4
```

### Zasady nazewnictwa

| Typ | Wzorzec ścieżki | Identyfikator w nazwie |
|-----|-----------------|------------------------|
| Miniaturka | `src/pictures/cats/{CatId}/thumbnail/{CatImageId}.webp` | `CatImageId` (z Value Object) |
| Galeria | `src/pictures/cats/{CatId}/gallery/{CatGalleryItemId}.{ext}` | `CatGalleryItemId` (z Entity) |

### Rozszerzenia plików

- **Miniaturka:** `.webp` (zawsze)
- **Galeria:** `.webp` lub `.mp4` (wykrywane przez infrastrukturę)

### Separacja odpowiedzialności

| Warstwa | Odpowiedzialność |
|---------|------------------|
| **Domena** | Generuje ID, zarządza kolejnością, wymusza limity |
| **Application** | Koordynuje upload z zapisem do bazy, waliduje format pliku |
| **Infrastruktura** | Fizyczny zapis plików według konwencji, odczyt, usuwanie |

---

## Flow biznesowe

### 1. Ustawienie miniaturki

#### Sekwencja operacji:

```
1. Application Layer otrzymuje IFormFile
2. Walidacja: czy plik to obraz (.jpg/.png/.webp)
3. Domena: Cat.UpdateThumbnail(CatThumbnail.Create())
4. Infrastruktura: Zapis pliku do src/pictures/cats/{CatId}/thumbnail/{CatImageId}.webp
5. Commit: UnitOfWork.SaveChangesAsync()
```

#### Kod domeny:

```csharp
public Result UpdateThumbnail(CatThumbnail thumbnail)
{
    ArgumentNullException.ThrowIfNull(thumbnail);
    Thumbnail = thumbnail;
    return Result.Success();
}
```

#### Command Handler (pseudo-kod):

```csharp
Result<CatThumbnail> thumbnailResult = CatThumbnail.Create();
Result updateResult = cat.UpdateThumbnail(thumbnailResult.Value);

if (updateResult.IsSuccess)
{
    await _fileStorage.SaveThumbnail(
        cat.Id, 
        thumbnailResult.Value.CatImageId, 
        command.File);
    
    await _unitOfWork.SaveChangesAsync();
}
```

#### Zachowanie przy zmianie miniaturki:

- Stara miniaturka pozostaje na dysku (orphaned file)
- Nowe `CatImageId` generuje nową ścieżkę
- Cleanup job okresowo usuwa orphaned pliki

---

### 2. Dodanie elementu do galerii

#### Sekwencja operacji:

```
1. Application Layer otrzymuje IFormFile
2. Domena: Result<CatGalleryItemId> result = cat.AddGalleryItem()
3. Domena zwraca nowe CatGalleryItemId
4. Infrastruktura: Zapis pliku do src/pictures/cats/{CatId}/gallery/{CatGalleryItemId}.{ext}
5. Commit: UnitOfWork.SaveChangesAsync()
```

#### Kod domeny:

```csharp
public Result<CatGalleryItemId> AddGalleryItem()
{
    if (_galleryItems.Count >= MaximumGalleryItemsCount)
    {
        return Result.Failure<CatGalleryItemId>(DomainErrors.Cat.GalleryIsFull);
    }

    int nextDisplayOrder = _galleryItems.Count;
    Result<CatGalleryItemDisplayOrder> displayOrderResult =
        CatGalleryItemDisplayOrder.Create(nextDisplayOrder, MaximumGalleryItemsCount);

    if (displayOrderResult.IsFailure)
    {
        return Result.Failure<CatGalleryItemId>(displayOrderResult.Error);
    }

    Result<CatGalleryItem> galleryItemCreationResult =
        CatGalleryItem.Create(displayOrderResult.Value, CreatedAt);

    if (galleryItemCreationResult.IsFailure)
    {
        return Result.Failure<CatGalleryItemId>(galleryItemCreationResult.Error);
    }

    _galleryItems.Add(galleryItemCreationResult.Value);
    return Result.Success(galleryItemCreationResult.Value.Id);
}
```

#### Command Handler (pseudo-kod):

```csharp
Result<CatGalleryItemId> result = cat.AddGalleryItem();

if (result.IsSuccess)
{
    CatGalleryItemId itemId = result.Value;
    
    await _fileStorage.SaveGalleryItem(
        cat.Id, 
        itemId, 
        command.File);
    
    await _unitOfWork.SaveChangesAsync();
    // Jeśli SaveChanges jebnie - orphaned file, cleanup job ogarnie
}
```

#### Automatyczne przypisanie `DisplayOrder`:

- Nowy element dostaje `DisplayOrder = _galleryItems.Count`
- Zapewnia ciągłość indeksów: `[0, 1, 2, ..., N-1]`

---

### 3. Zmiana kolejności galerii (Drag & Drop)

#### Sekwencja operacji:

```
1. Frontend przesyła mapę: Dictionary<CatGalleryItemId, int>
2. Application Layer tworzy Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder>
3. Domena: Result result = cat.ReorderGalleryItems(newOrders)
4. Commit: UnitOfWork.SaveChangesAsync()
```

#### Kod domeny:

```csharp
public Result ReorderGalleryItems(Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders)
{
    ArgumentNullException.ThrowIfNull(newOrders);

    if (newOrders.Count != _galleryItems.Count)
    {
        return Result.Failure(DomainErrors.Cat.InvalidReorderOperation);
    }

    HashSet<int> displayOrderValues = newOrders.Values.Select(o => o.Value).ToHashSet();
    if (displayOrderValues.Count != newOrders.Count)
    {
        return Result.Failure(DomainErrors.Cat.DuplicateDisplayOrders);
    }

    int minOrder = displayOrderValues.Min();
    int maxOrder = displayOrderValues.Max();
    if (minOrder != 0 || maxOrder != _galleryItems.Count - 1)
    {
        return Result.Failure(DomainErrors.Cat.DisplayOrderMustBeContiguous);
    }

    foreach (KeyValuePair<CatGalleryItemId, CatGalleryItemDisplayOrder> kvp in newOrders)
    {
        Maybe<CatGalleryItem> maybeItem = _galleryItems.GetByIdOrDefault(kvp.Key);
        if (maybeItem.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatGalleryItem.NotFound(kvp.Key));
        }

        maybeItem.Value.UpdateDisplayOrder(kvp.Value);
    }

    return Result.Success();
}
```

#### Walidacje:

| Walidacja | Opis | Błąd |
|-----------|------|------|
| Kompletność | Liczba nowych pozycji = liczba elementów | `InvalidReorderOperation` |
| Unikalność | Brak duplikatów w `DisplayOrder` | `DuplicateDisplayOrders` |
| Ciągłość | Zakres `[0, N-1]` bez dziur | `DisplayOrderMustBeContiguous` |
| Istnienie | Wszystkie ID istnieją w galerii | `CatGalleryItem.NotFound` |

---

### 4. Usunięcie elementu z galerii

#### Sekwencja operacji:

```
1. Application Layer wywołuje: cat.RemoveGalleryItem(galleryItemId)
2. Domena usuwa element i przebudowuje DisplayOrder
3. Infrastruktura: Usuwa plik src/pictures/cats/{CatId}/gallery/{CatGalleryItemId}.{ext}
4. Commit: UnitOfWork.SaveChangesAsync()
```

#### Kod domeny:

```csharp
public Result RemoveGalleryItem(CatGalleryItemId galleryItemId)
{
    Ensure.NotEmpty(galleryItemId);

    Maybe<CatGalleryItem> maybeGalleryItem = _galleryItems.GetByIdOrDefault(galleryItemId);
    if (maybeGalleryItem.HasNoValue)
    {
        return Result.Failure(DomainErrors.CatGalleryItem.NotFound(galleryItemId));
    }

    int removedDisplayOrder = maybeGalleryItem.Value.DisplayOrder.Value;
    _galleryItems.Remove(maybeGalleryItem.Value);

    foreach (CatGalleryItem item in _galleryItems.Where(i => i.DisplayOrder.Value > removedDisplayOrder))
    {
        Result<CatGalleryItemDisplayOrder> newOrderResult =
            CatGalleryItemDisplayOrder.Create(item.DisplayOrder.Value - 1, MaximumGalleryItemsCount);

        if (newOrderResult.IsFailure)
        {
            return Result.Failure(newOrderResult.Error);
        }

        item.UpdateDisplayOrder(newOrderResult.Value);
    }

    return Result.Success();
}
```

#### Automatyczne przebudowanie indeksów:

**Przed usunięciem:**
```
[0] Item-A
[1] Item-B  ← USUWANY
[2] Item-C
[3] Item-D
```

**Po usunięciu:**
```
[0] Item-A
[1] Item-C  ← było [2]
[2] Item-D  ← było [3]
```

Elementy po usuniętym mają `DisplayOrder` dekrementowany o 1, zachowując ciągłość.

---

## Invarianty domenowe

### Agregat Cat - Miniaturka

| Invariant | Wymuszenie | Miejsce |
|-----------|------------|---------|
| Miniaturka jest opcjonalna | `CatThumbnail?` | `Cat` |
| Miniaturka ma unikalny `CatImageId` | Generowane przez `CatImageId.New()` | `CatThumbnail.Create()` |

### Agregat Cat - Galeria

| Invariant | Wymuszenie | Miejsce |
|-----------|------------|---------|
| Maksymalnie 20 elementów | `MaximumGalleryItemsCount = 20` | `Cat.AddGalleryItem()` |
| `DisplayOrder` zaczyna się od 0 | `MinValue = 0` | `CatGalleryItemDisplayOrder` |
| `DisplayOrder` musi być ciągły | Walidacja zakresu `[0, N-1]` | `Cat.ReorderGalleryItems()` |
| Brak duplikatów `DisplayOrder` | Walidacja unikalności | `Cat.ReorderGalleryItems()` |
| Elementy mają unikalne ID | Generowane przez `CatGalleryItemId.New()` | `CatGalleryItem.Create()` |
| Usunięcie przebudowuje indeksy | Automatyczna dekrementacja | `Cat.RemoveGalleryItem()` |

---

## Przykłady użycia

### Przykład 1: Upload miniaturki

```csharp
// Command
public record UploadCatThumbnailCommand(CatId CatId, IFormFile File);

// Handler
public async Task<Result> Handle(UploadCatThumbnailCommand command, CancellationToken ct)
{
    Cat? cat = await _catRepository.GetByIdAsync(command.CatId, ct);
    if (cat is null)
    {
        return Result.Failure(DomainErrors.Cat.NotFound(command.CatId));
    }

    if (command.File.ContentType is not "image/jpeg" and not "image/png" and not "image/webp")
    {
        return Result.Failure(ValidationErrors.InvalidImageFormat);
    }

    Result<CatThumbnail> thumbnailResult = CatThumbnail.Create();
    if (thumbnailResult.IsFailure)
    {
        return thumbnailResult;
    }

    Result updateResult = cat.UpdateThumbnail(thumbnailResult.Value);
    if (updateResult.IsFailure)
    {
        return updateResult;
    }

    await _fileStorage.SaveCatThumbnail(
        cat.Id, 
        thumbnailResult.Value.CatImageId, 
        command.File, 
        ct);

    await _unitOfWork.SaveChangesAsync(ct);

    return Result.Success();
}
```

---

### Przykład 2: Upload do galerii

```csharp
// Command
public record UploadCatGalleryItemCommand(CatId CatId, IFormFile File);

// Handler
public async Task<Result<CatGalleryItemId>> Handle(
    UploadCatGalleryItemCommand command, 
    CancellationToken ct)
{
    Cat? cat = await _catRepository.GetByIdAsync(command.CatId, ct);
    if (cat is null)
    {
        return Result.Failure<CatGalleryItemId>(DomainErrors.Cat.NotFound(command.CatId));
    }

    string extension = Path.GetExtension(command.File.FileName).ToLowerInvariant();
    if (extension is not ".jpg" and not ".png" and not ".webp" and not ".mp4")
    {
        return Result.Failure<CatGalleryItemId>(ValidationErrors.InvalidMediaFormat);
    }

    Result<CatGalleryItemId> addResult = cat.AddGalleryItem();
    if (addResult.IsFailure)
    {
        return addResult;
    }

    CatGalleryItemId itemId = addResult.Value;

    await _fileStorage.SaveCatGalleryItem(
        cat.Id, 
        itemId, 
        command.File, 
        ct);

    await _unitOfWork.SaveChangesAsync(ct);

    return Result.Success(itemId);
}
```

---

### Przykład 3: Drag & Drop (reorder)

```csharp
// Command
public record ReorderCatGalleryCommand(
    CatId CatId, 
    Dictionary<CatGalleryItemId, int> NewPositions);

// Handler
public async Task<Result> Handle(ReorderCatGalleryCommand command, CancellationToken ct)
{
    Cat? cat = await _catRepository.GetByIdAsync(command.CatId, ct);
    if (cat is null)
    {
        return Result.Failure(DomainErrors.Cat.NotFound(command.CatId));
    }

    Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders = new();

    foreach (KeyValuePair<CatGalleryItemId, int> kvp in command.NewPositions)
    {
        Result<CatGalleryItemDisplayOrder> orderResult = 
            CatGalleryItemDisplayOrder.Create(kvp.Value, Cat.MaximumGalleryItemsCount);
        
        if (orderResult.IsFailure)
        {
            return orderResult;
        }

        newOrders[kvp.Key] = orderResult.Value;
    }

    Result reorderResult = cat.ReorderGalleryItems(newOrders);
    if (reorderResult.IsFailure)
    {
        return reorderResult;
    }

    await _unitOfWork.SaveChangesAsync(ct);

    return Result.Success();
}
```

---

### Przykład 4: Usunięcie z galerii

```csharp
// Command
public record RemoveCatGalleryItemCommand(CatId CatId, CatGalleryItemId GalleryItemId);

// Handler
public async Task<Result> Handle(RemoveCatGalleryItemCommand command, CancellationToken ct)
{
    Cat? cat = await _catRepository.GetByIdAsync(command.CatId, ct);
    if (cat is null)
    {
        return Result.Failure(DomainErrors.Cat.NotFound(command.CatId));
    }

    Result removeResult = cat.RemoveGalleryItem(command.GalleryItemId);
    if (removeResult.IsFailure)
    {
        return removeResult;
    }

    await _fileStorage.DeleteCatGalleryItem(cat.Id, command.GalleryItemId, ct);

    await _unitOfWork.SaveChangesAsync(ct);

    return Result.Success();
}
```

---

## Obsługa błędów

### Błędy galerii

**Lokalizacja:** `DomainErrors.Cat` i `DomainErrors.CatGalleryItem`

| Kod błędu | Scenariusz | Komunikat |
|-----------|------------|-----------|
| `Cat.GalleryIsFull` | Próba dodania 21. elementu | "Gallery has reached maximum capacity of 20 items" |
| `Cat.InvalidReorderOperation` | Liczba pozycji ≠ liczba elementów | "Reorder operation must include all gallery items" |
| `Cat.DuplicateDisplayOrders` | Duplikaty w `DisplayOrder` | "Display orders must be unique" |
| `Cat.DisplayOrderMustBeContiguous` | Zakres nie jest `[0, N-1]` | "Display orders must be contiguous starting from 0" |
| `CatGalleryItem.NotFound` | Element nie istnieje | "Gallery item with ID {id} not found" |
| `CatGalleryItem.DisplayOrder.BelowMinimum` | `DisplayOrder < 0` | "Display order {value} is below minimum {min}" |
| `CatGalleryItem.DisplayOrder.AboveOrEqualMaximum` | `DisplayOrder >= MaxCount` | "Display order {value} exceeds maximum {max}" |

---

## Edge case'y i rozwiązania

### 1. SaveChanges jebnie po uploadzie

**Problem:** Plik zapisany, baza nie.

**Rozwiązanie:**
```csharp
try
{
    await _unitOfWork.SaveChangesAsync(ct);
}
catch
{
    await _fileStorage.DeleteCatGalleryItem(cat.Id, itemId, ct);
    throw;
}
```

**Backup:** Cleanup job usuwa orphaned pliki bez rekordów w bazie.

---

### 2. Upload jebnie po domenowej metodzie

**Problem:** `AddGalleryItem()` sukces, upload fail.

**Rozwiązanie:** Transakcja się nie commituje, rollback automatyczny. Zero side effectów.

---

### 3. Orphaned files (plik bez rekordu)

**Scenariusz:** `SaveChangesAsync()` jebnie po uplodie.

**Rozwiązanie:** Background job:
```csharp
// 1. Wczytaj wszystkie CatGalleryItemId z bazy
// 2. Zeskanuj katalog src/pictures/cats/{catId}/gallery/
// 3. Usuń pliki, których ID nie ma w bazie
```

---

### 4. Zmiana miniaturki - stara pozostaje

**Scenariusz:** User uploaduje nową miniaturkę, stara zostaje na dysku.

**Rozwiązanie:**
- **Akceptowalne** - stare miniaturki nie przeszkadzają (różne `CatImageId`)
- **Opcjonalnie:** Cleanup job usuwa miniaturki starsze niż X dni

---

## Pytania i odpowiedzi

### Q: Dlaczego `CatThumbnail` to Value Object, a `CatGalleryItem` to Entity?

**A:** 
- **Miniaturka** - brak tożsamości biznesowej. Liczy się tylko ID zdjęcia. Wymiana = nowy obiekt.
- **Element galerii** - ma tożsamość (`CatGalleryItemId`). Ten sam element może zmieniać pozycję przez `UpdateDisplayOrder`.

---

### Q: Dlaczego domena nie wie o `.webp` / `.mp4`?

**A:** 
- DDD: Domena modeluje biznes, nie technikalia
- Enkapsulacja: Format pliku to szczegół infrastruktury
- Elastyczność: Możemy zmienić format bez zmiany domeny

---

### Q: Jak frontend wie czy element to obraz czy film?

**A:** 
Infrastruktura zwraca URL z extensionem:
```json
{
  "galleryItems": [
    { "id": "guid-1", "url": "/cats/{catId}/gallery/{id}.webp" },
    { "id": "guid-2", "url": "/cats/{catId}/gallery/{id}.mp4" }
  ]
}
```

Frontend parsuje `.webp` → `<img>`, `.mp4` → `<video>`.

---

### Q: Czy mogę mieć limity typu "max 3 filmy"?

**A:** 
TAK, ale wymaga dodania `MediaType` do `CatGalleryItem`:

```csharp
public sealed class CatGalleryItem : Entity<CatGalleryItemId>
{
    public CatGalleryItemDisplayOrder DisplayOrder { get; private set; }
    public CatGalleryItemMediaType MediaType { get; private set; }
}

public Result<CatGalleryItemId> AddGalleryItem(CatGalleryItemMediaType mediaType)
{
    int videoCount = _galleryItems.Count(x => x.MediaType == CatGalleryItemMediaType.Video);
    if (mediaType == CatGalleryItemMediaType.Video && videoCount >= 3)
    {
        return Result.Failure<CatGalleryItemId>(DomainErrors.Cat.TooManyVideos);
    }
    
    // ... reszta logiki
}
```

---

## Diagram flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Agregat Cat                              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Thumbnail (CatThumbnail? - Value Object)                  │
│  ┌──────────────────────────────────────────────┐          │
│  │ CatImageId                                    │          │
│  └──────────────────────────────────────────────┘          │
│                         │                                   │
│                         │ Maps to storage:                  │
│                         ├─► src/pictures/cats/{CatId}/      │
│                         │     thumbnail/{CatImageId}.webp   │
│                                                             │
│  GalleryItems (List<CatGalleryItem> - Entities)            │
│  ┌──────────────────────────────────────────────┐          │
│  │ [0] CatGalleryItemId-A  DisplayOrder=0       │          │
│  │ [1] CatGalleryItemId-B  DisplayOrder=1       │          │
│  │ [2] CatGalleryItemId-C  DisplayOrder=2       │          │
│  │ ...                                           │          │
│  │ [19] CatGalleryItemId-T DisplayOrder=19      │          │
│  └──────────────────────────────────────────────┘          │
│                         │                                   │
│                         │ Maps to storage:                  │
│                         ├─► src/pictures/cats/{CatId}/      │
│                         │     gallery/{ItemId}.webp|.mp4    │
│                                                             │
│  Max: 20 items                                              │
│  DisplayOrder: [0, 19] - ciągły, bez duplikatów            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Statystyki

| Metryka | Wartość |
|---------|---------|
| Maksymalna liczba elementów galerii | 20 |
| Minimalna pozycja `DisplayOrder` | 0 |
| Maksymalna pozycja `DisplayOrder` | 19 |
| Encje związane z mediami | 1 (`CatGalleryItem`) |
| Value Objects związane z mediami | 2 (`CatThumbnail`, `CatGalleryItemDisplayOrder`) |
| Publiczne metody w `Cat` dla mediów | 4 (`UpdateThumbnail`, `AddGalleryItem`, `RemoveGalleryItem`, `ReorderGalleryItems`) |
