# ChainTrack — DZ3 (Ugradnja odabrane arhitekture)

Programsko rješenje za DZ3 kolegija INFSUS.
**Slojevita arhitektura (N-tier)** — ASP.NET Core 8 MVC + Entity Framework Core (SQLite / PostgreSQL).

## Arhitektura

4 projekta unutar `ChainTrack.sln`:

| Sloj | Projekt |
|---|---|
| Domenski sloj | `src/ChainTrack.Domain` |
| Sloj za pristup podacima (DAL) | `src/ChainTrack.DAL` |
| Poslovni sloj (BLL) | `src/ChainTrack.BLL` |
| Prezentacijski sloj (MVC) | `src/ChainTrack.Web` |
| Testovi (xUnit) | `tests/ChainTrack.Tests` |

Dijagram komponenti: **[`docs/arhitektura.png`](docs/arhitektura.png)**
(izvor: `docs/generate_arhitektura.py`).

## Funkcionalnosti

- **Master-detail ekran** — `Lokacije`: zaglavlje (lokacija + voditelj iz FK padajuće liste) i detalji (`ZalihaSastojka` po lokaciji s FK na `Sastojak`).
- **Šifrarnik** — `Zaposlenici` (FK na `TipZaposlenika` kroz padajuću listu).
- Navigacija i pretraživanje, CRUD na svim ekranima.
- **Složena validacija** — provjera HR OIB-a kontrolnom znamenkom (ISO 7064, MOD 11,10) u `ChainTrack.BLL/Validation/OibValidator.cs`.

---

## Pokretanje lokalno (SQLite — preporučeno, bez ikakve vanjske baze)

### 1. Preduvjeti

Treba ti **.NET SDK** instaliran (provjera: `dotnet --version`).

- Ako nemaš ništa → instaliraj .NET 8 SDK s https://dotnet.microsoft.com/download/dotnet/8.0 (macOS Arm64 / x64 installer) ili `brew install --cask dotnet-sdk`.
- Ako već imaš **.NET 10** SDK (a projekt cilja net8.0), aplikacija će se pokrenuti uz `DOTNET_ROLL_FORWARD=LatestMajor` (vidi dolje).

### 2. Kloniranje i pokretanje

```bash
git clone https://github.com/<korisnik>/treci-labos.git
cd treci-labos/src/ChainTrack.Web
dotnet run -- --DatabaseProvider=Sqlite
```

Ako imaš samo .NET 10 SDK (bez 8 runtime-a), umjesto gornjeg zadnjeg reda pokreni:

```bash
DOTNET_ROLL_FORWARD=LatestMajor dotnet run -- --DatabaseProvider=Sqlite
```

### 3. Otvaranje aplikacije

Konzola ispiše nešto poput:

```
Now listening on: http://localhost:5xxx
```

Otvori taj URL u pregledniku. SQLite baza (`chaintrack.db`) se sama kreira u radnom direktoriju i puni oglednim podacima preko `DatabaseSeeder.SeedAsync` — nema dodatne konfiguracije.

### Alternativa: PostgreSQL

Ako želiš pokrenuti s Postgresom (zadano u `appsettings.json`), podigni Postgres na `localhost:5432` (user `postgres`, pass `postgres`, baza `chaintrack`) i samo:

```bash
cd src/ChainTrack.Web
dotnet run
```

---

## Pokretanje testova

```bash
cd treci-labos
dotnet test
```

Testovi koriste SQLite in-memory bazu (`SqliteTestContext`) i `WebApplicationFactory<Program>` za integracijske testove — **ponovljivi su**, mogu se pokretati više puta uzastopno.

Pokriveni slojevi:

- **BLL jedinični** — `tests/.../Bll/` (`OibValidatorTests`, `LokacijaServiceTests`, `ZaposlenikServiceTests`)
- **DAL jedinični** — `tests/.../Dal/` (Lokacija/Zaposlenik/ZalihaSastojka repozitoriji)
- **Prezentacijski jedinični** — `tests/.../Web/` (`LokacijeControllerTests`, `ZaposleniciControllerTests`)
- **Integracijski** — preko `KontrolerTestPostavke` (cijeli stack na SQLite-u)
