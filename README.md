# Autorzy:
Jan Kopacz
Szymon Pocheć

# Link do Github
https://github.com/Neri-Git/Backend

# README

## Warstwa domenowa i Core

- Encje bazowe dziedziczące po `EntityBase (Guid)`
- Model domeny:
    - `Person`
    - `Contact`
    - `Company`
    - `Organization`
- Typy pomocnicze:
    - `Address`
    - `Note`
    - enumy (statusy, typy, role)

---

## DTO i warstwa transportowa

- DTO dla:
    - Person (read/create/update)
    - Contact
- `ContactSearchDto` (wyszukiwanie)
- `PagedResult<T>` (paginacja)
- Separacja encji od API (brak bezpośredniego exposure modeli domenowych)

---

## Repozytoria

- `IGenericRepositoryAsync<T>`
- Repozytoria domenowe:
    - Person
    - Contact
    - Company
    - Organization
- Operacje:
    - CRUD
    - paginacja
    - wyszukiwanie
- Dodatkowe operacje:
    - tagi
    - notatki
    - filtrowanie

---

## Implementacja In-Memory

- `MemoryGenericRepository<T>`
- implementacje repozytoriów w pamięci
- brak trwałości danych
- brak współbieżności (single-thread assumption)
- brak mechanizmów transakcyjnych

---

## Unit of Work

- `IContactUnitOfWork`
- `MemoryContactUnitOfWork`
- centralizacja dostępu do repozytoriów
- spójne zarządzanie operacjami domenowymi

---

## Serwisy aplikacyjne

- `IPersonService`
- `MemoryPersonService`

Operacje:
- pobieranie danych (z paginacją)
- tworzenie / edycja / usuwanie
- notatki i tagi
- ręczne mapowanie DTO ↔ encje

---

## Web API

### Kontrolery
- `ContactsController`
- `StudentsController`

### Endpointy
- CRUD operacje REST
- statusy HTTP:
    - 200 OK
    - 201 Created
    - 400 Bad Request
    - 404 Not Found

---

## Walidacja

- FluentValidation
- walidacja DTO w pipeline API

Reguły:
- poprawność email
- długości pól
- ograniczenia znaków
- podstawowa spójność danych

---

## Obsługa błędów

- własne wyjątki domenowe:
    - np. `ContactNotFoundException`
- globalny handler:
    - `ProblemDetailsExceptionHandler`
- standard RFC 7807 (Problem Details)

---

## Uwagi techniczne

- Model domeny niespójny:
    - `Person`, `Student`, `Contact` częściowo dublują odpowiedzialności
- Mapowanie ręczne:
    - wysokie ryzyko duplikacji i błędów
- In-memory:
    - brak trwałości danych
    - brak równoległości
    - brak mechanizmów produkcyjnych (np. transakcji)
========================
LAB 6 — EF CORE + IDENTITY
========================
- Instalacja: EF Core (SQLite), Identity, Design
- Struktura Infrastructure: Entities / Repositories / UnitOfWork / Context
- Identity:
    - CrmUser : IdentityUser + ISystemUser
    - CrmRole : IdentityRole
- DbContext:
    - IdentityDbContext + TPH (Contact → Person/Company/Organization)
    - Seed danych (Person, Company, Organization)
    - Relacje: Person–Company, Organization–Members
- Repozytoria:
    - Generic EF repository (CRUD + paging)
    - Repozytoria domenowe (Company, Person itd.)
- UnitOfWork:
    - agregacja repozytoriów + SaveChanges + transakcje
- DI:
    - AddContactsEfModule()
    - przejście z Memory → EF
- Migracje EF + SQLite connection string

UWAGA:
- DateTime.Now → powinno być UtcNow
- Seed ID musi być stałe (Guid ręcznie)

========================
LAB 7 — JWT AUTH
========================
- Paczki: JwtBearer + System.IdentityModel.Tokens.Jwt
- JwtSettings:
    - konfiguracja z appsettings.json
    - Secret → powinien być w env (nie w config produkcyjnym)
- DTO:
    - LoginDto
    - AuthResponseDto
    - RefreshTokenDto
- RefreshToken:
    - encja + DbSet
    - rotacja tokenów (replace + revoke)
- AuthService:
    - Login → access + refresh token
    - RefreshToken → walidacja + rotacja
    - Revoke
- JWT:
    - claims: email, role, department, id
- Authorization:
    - polityki oparte o role + claims
    - AddJwt() (Bearer + policies)
- AuthController:
    - login / refresh / revoke / me
- Seeder Identity:
    - role + user seed

UWAGA:
- literówka: RefreshTokens (w tekście było RefresTokens)
- fallback policy = default policy (redundantne)

========================
LAB 8 — TESTY INTEGRACYJNE
========================
- xUnit + WebApplicationFactory<Program>
- Testy API przez HttpClient:
    - GET /api/...
    - status code check
    - content-type check
- Program.cs:
    - public partial class Program
- InMemory DB:
    - UseInMemoryDatabase("TestDb")
    - podmiana DbContext w factory
- Custom TestFactory:
    - usuwa real DB
    - rejestruje InMemory
- Seed testowy:
    - dane w konstruktorze testów
