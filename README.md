
# Projekt CRM

## Autorzy
- **Szymon Pocheć**
- **Kopacz Jan**

## Repozytorium GitHub
https://github.com/Neri-Git/Backend

## Opis projektu
Aplikacja jest przykładowym systemem CRM zbudowanym jako ASP.NET Core Web API. Projekt wykorzystuje czystą architekturę i rozdział na warstwy:
- `AppCore` – modele domenowe, DTO, walidatory, interfejsy i logika aplikacyjna
- `Infrastructure` – EF Core, SQLite, Identity, JWT, refresh tokeny, seedery i serwisy infrastrukturalne
- `WebAPI` – kontrolery REST, obsługa wyjątków i punkt wejścia API
- `AppCore.Tests` – testy integracyjne aplikacji

System obsługuje zarządzanie kontaktami, autoryzację użytkowników, impor­t danych z CSV/JSON oraz wstępne dane startowe potrzebne do testowania.

## Najważniejsze implementacje
### 1. Warstwa domenowa i API
- W `AppCore` znajdują się modele `Person`, `Company`, `Organization`, `Contact`, a także DTO do tworzenia, odczytu i aktualizacji kontaktów.
- `PeopleController` udostępnia API dla osób: listowanie, pobieranie po id, tworzenie, aktualizację oraz dodawanie i pobieranie notatek.
- `CompaniesController` i `OrganizationsController` udostępniają odczyt danych dla firm i organizacji.

### 2. Autoryzacja i bezpieczeństwo
- `AuthController` obsługuje:
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `POST /api/auth/revoke`
  - `GET /api/auth/me`
- `AuthService` generuje JWT oraz refresh tokeny, sprawdza hasła, blokuje nieaktywnych użytkowników i unieważnia stare tokeny przy odświeżaniu.
- `ContactsInfrastructureModule` rejestruje `Identity`, polityki autoryzacji oraz mechanizm JWT.
- Użytkownicy mają role (`Administrator`, `SalesManager`, `Salesperson`, `SupportAgent`, `ReadOnly`) i są przechowywani w bazie przez `IdentityDbContext`.

### 3. Baza danych i relacje
- Projekt używa SQLite z EF Core.
- `ContactsDbContext` łączy `IdentityDbContext` z tabelami kontaktów i refresh tokenów.
- Kontaktowe encje są dziedziczone po wspólnej bazie `Contact`, a typ kontaktu jest przechowywany jako dyskryminator (`Person`, `Company`, `Organization`).
- Pole `CreatedByUserId` jest zapisywane przy rekordach kontaktów, co pozwala wiązać import lub tworzenie z użytkownikiem.

### 4. Import kontaktów
- `ContactImportService` wspiera import z plików `CSV` i `JSON` przez endpoint `POST /api/contact-import`.
- Obsługuje rozpoznawanie delimitera, mapowanie danych do odpowiednich typów kontaktów, walidację wejścia i raportowanie błędów.
- Import przypisuje `CreatedByUserId` do nowo dodanych kontaktów, dzięki czemu można śledzić użytkownika, który zaimportował dane.

### 5. Seedery i dane startowe
- `IdentityDbSeeder` tworzy przykładowych użytkowników i role przy starcie aplikacji.
- `PeopleDbSeeder` dodaje przykładowe kontakty typu `Person`.
- Dzięki temu API ma od razu dane do testowania bez ręcznego przygotowywania bazy.

### 6. Testy
- `AppCore.Tests` zawiera testy integracyjne uruchamiane z użyciem fabryki aplikacji testowej.
- Testy sprawdzają podstawowe ścieżki REST, m.in. pobieranie osoby, tworzenie osoby, aktualizację i dodawanie notatki.

## Struktura projektu
- `AppCore` – domena, DTO, walidatory, interfejsy, modele
- `Infrastructure` – EF Core, SQLite, Identity, JWT, refresh tokeny, seedery, usługi
- `WebAPI` – kontrolery, konfiguracja, endpointy, pliki importu, `WebAPI.http`
- `AppCore.Tests` – testy integracyjne

## Uruchomienie projektu
1. Uruchom aplikację:
   `dotnet run --project WebAPI`
2. Aplikacja będzie korzystać z SQLite (`contacts.db`) i wstępnych seedów.
3. Najważniejsze endpointy:
   - `POST /api/auth/login` – logowanie i otrzymanie tokenu JWT
   - `POST /api/auth/refresh` – odświeżenie tokenu, przy użyciu refresh tokenu
   - `POST /api/auth/revoke` – wylogowanie i unieważnienie refresh tokenu
   - `GET /api/people` – pobranie listy osób
   - `POST /api/people` – utworzenie osoby
   - `PUT /api/people/{id}` – aktualizacja osoby
   - `POST /api/people/{id}/notes` – dodanie notatki do osoby
   - `GET /api/people/{id}/notes` – pobranie notatek osoby
   - `POST /api/contact-import` – import CSV/JSON
   - `GET /api/companies` i `GET /api/organizations` – odczyt danych firm i organizacji

## Przykładowe dane startowe
Po uruchomieniu aplikacji zostają utworzeni przykładowi użytkownicy, m.in. administrator i użytkownicy z rolami sprzedaży oraz wsparcia. Dzięki temu można testować autoryzację i role bez dodatkowej konfiguracji.

## Testowanie i import
- Requesty HTTP można uruchamiać z pliku `WebAPI/WebAPI.http`.
- Pliki wspierające import znajdują się w katalogu `WebAPI` (`JsonImport.json`, `JsonImportErrorTest.json`, `CsvImport.csv`, `CsvImportErrorTest.csv`, `DelimiterTest.csv`).
- Testy uruchamiasz poleceniem:
  `dotnet test AppCore.Tests/AppCore.Tests.csproj`

## Uwaga
Projekt łączy szereg kluczowych elementów CRM: autoryzację, role, refresh tokeny, bazę SQLite, relacje kontaktów, import danych oraz testy integracyjne. To sprawia, że jest gotowym przykładem aplikacji webowej z realną warstwą API i bezpieczeństwem.
