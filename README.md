
# Projekt 3

## Autorzy

**Szymon Pocheć**
**Kopacz jan**

## Repozytorium GitHub

Link do repozytorium:

```text
https://github.com/Neri-Git/Backend
```

## Opis aplikacji
Aplikacja jest przykładowym systemem CRM do zarządzania kontaktami. Projekt został przygotowany jako aplikacja WebAPI w technologii ASP.NET Core. System pozwala na obsługę różnych typów kontaktów, takich jak osoby, firmy oraz organizacje.

Aplikacja została zbudowana z wykorzystaniem podziału na warstwy zgodnie z założeniami czystej architektury. Logika domenowa, modele, DTO, interfejsy oraz walidatory znajdują się w warstwie AppCore. Implementacja dostępu do danych, konfiguracja Entity Framework, Identity, JWT oraz serwisy infrastrukturalne znajdują się w warstwie Infrastructure. Aplikacja udostępniająca REST API znajduje się w projekcie WebAPI. Dodatkowo projekt posiada część testową przeznaczoną do testów integracyjnych i end-to-end.

System wykorzystuje bazę danych SQLite oraz Entity Framework Core. Dostęp do aplikacji jest zabezpieczony z użyciem ASP.NET Core Identity, ról użytkowników, JWT oraz refresh tokenów. Każdy kontakt może posiadać informację o użytkowniku, który go dodał lub zaimportował.

## Technologie użyte w projekcie
W projekcie wykorzystano:
```text
ASP.NET Core WebAPI
Entity Framework Core
SQLite
ASP.NET Core Identity
JWT Bearer Authentication
Refresh Token
FluentValidation
ProblemDetails
REST API
Testy integracyjne
```

## Struktura projektu
Projekt został podzielony na następujące części:
```text
PabLaboratory1
├── AppCore
├── AppCore.Tests
├── Infrastructure
└── WebAPI
```
## AppCore
Projekt AppCore zawiera główną logikę aplikacji oraz elementy niezależne od infrastruktury.

Najważniejsze foldery:
```text
AppCore
├── Authorization
├── Common
├── Dto
├── Exceptions
├── Interfaces
├── Models
├── Module
├── Services
├── Validators
└── ValueObjects
```
W projekcie AppCore znajdują się między innymi:
```text
modele domenowe, np. Person, Company, Organization, Contact
klasy DTO, np. PersonDto, CompanyDto, OrganizationDto, CreatePersonDto, CreateCompanyDto
interfejsy repozytoriów i serwisów
walidatory FluentValidation
wyjątki domenowe, np. ContactNotFoundException
obiekty wartości, np. Address, Gender, ContactStatus, OrganizationType
logika serwisów aplikacyjnych
```

## Infrastructure
Projekt Infrastructure zawiera implementację dostępu do danych i mechanizmy infrastrukturalne.

Najważniejsze foldery:
```text
Infrastructure
├── EntityFramework
│   ├── Context
│   ├── Entities
│   ├── Repositories
│   └── UnitOfWork
├── Memory
├── Migrations
├── Security
├── Seeders
└── Services
```
W projekcie Infrastructure znajdują się między innymi:
```text
ContactsDbContext
konfiguracja Entity Framework Core
konfiguracja SQLite
migracje bazy danych
encje Identity, np. CrmUser i CrmRole
repozytoria EF
UnitOfWork
serwisy infrastrukturalne, np. ContactImportService
obsługa JWT i refresh tokenów
seedery danych
```
## AppCore.Tests
Projekt AppCore.Tests zawiera testy aplikacji.

Najważniejsze elementy:
```text
testy jednostkowe
testy integracyjne
konfiguracja aplikacji testowej
ContactsAppTestFactory do uruchamiania aplikacji w środowisku testowym
```
## WebAPI
Projekt WebAPI udostępnia aplikację jako REST API.

Najważniejsze elementy:
```text
WebAPI
├── Controllers
├── Properties
├── appsettings.json
├── appsettings.Development.json
├── contacts.db
├── CsvImport.csv
├── CsvImportErrorTest.csv
├── DelimiterTest.csv
├── JsonImport.json
├── JsonImportErrorTest.json
├── ProblemDetailsExceptionHandler.cs
├── Program.cs
├── WeatherForecast.cs
└── WebAPI.http
```
W projekcie WebAPI znajdują się:
```text
kontrolery REST API
konfiguracja aplikacji
rejestracja zależności
obsługa wyjątków przez ProblemDetailsExceptionHandler
plik bazy SQLite contacts.db
pliki testowe do importu CSV i JSON
plik WebAPI.http do testowania endpointów
```
## Przykładowe dane testowe
W projekcie WebAPI dodano pliki testowe potrzebne do sprawdzenia importu kontaktów:
```text
CsvImport.csv
CsvImportErrorTest.csv
DelimiterTest.csv
JsonImport.json
JsonImportErrorTest.json
```

JsonImport.json
Plik zawiera poprawne dane testowe do importu kontaktów z formatu JSON.

W pliku mogą znajdować się trzy tablice:
```
{
  "people": [],
  "companies": [],
  "organizations": []
}
```
JsonImportErrorTest.json
Plik zawiera błędne dane testowe do sprawdzenia raportowania błędów importu JSON.

Przykładowe błędy:
```text
brak wymaganych pól
błędny format email
brak numeru telefonu
brak nazwy firmy lub organizacji
brak NIP dla firmy
```
CsvImport.csv
Plik zawiera poprawne dane testowe do importu kontaktów z formatu CSV.

Plik CSV może zawierać grupy:
```text
People
Companies
Organizations
```
CsvImportErrorTest.csv
Plik zawiera błędne dane testowe do sprawdzenia raportowania błędów importu CSV.

DelimiterTest.csv
Plik służy do sprawdzenia obsługi innego delimitera niż średnik.

Import CSV obsługuje delimiter, który nie jest literą, cyfrą ani znakiem specjalnym używanym w danych, takim jak:
```text
@
+
,
```
Przykładowe obsługiwane delimitery:
```text
;
|
#
:
tabulator
```
Jak przetestować import
Do testowania endpointów można użyć pliku:
```
WebAPI/WebAPI.http
```
Pliki testowe importu znajdują się bezpośrednio w projekcie WebAPI, dlatego w requestach można używać ścieżek względnych, np.:
```
< ./JsonImport.json
< ./CsvImport.csv
```
 Przykładowy Import poprawnego pliku JSON
```
POST http://localhost:5175/api/contact-import
Accept: application/json
Authorization: Bearer TOKEN
Content-Type: multipart/form-data; boundary=boundary

--boundary
Content-Disposition: form-data; name="file"; filename="JsonImport.json"
Content-Type: application/json

< ./JsonImport.json
--boundary--
```
