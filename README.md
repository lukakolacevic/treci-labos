# ChainTrack — DZ3 (Ugradnja odabrane arhitekture)

Programsko rješenje za DZ3 kolegija (slojevita arhitektura, N-tier).
ASP.NET Core 8 MVC + Entity Framework Core (PostgreSQL / SQLite).

## Arhitektura

Klasična slojevita arhitektura, 4 projekta unutar `ChainTrack.sln`:

| Sloj | Projekt |
|---|---|
| Domenski sloj | `src/ChainTrack.Domain` |
| Sloj za pristup podacima (DAL) | `src/ChainTrack.DAL` |
| Poslovni sloj (BLL) | `src/ChainTrack.BLL` |
| Prezentacijski sloj (MVC) | `src/ChainTrack.Web` |
| Testovi (xUnit) | `tests/ChainTrack.Tests` |

Dijagram komponenti: [`docs/arhitektura.png`](docs/arhitektura.png)
(izvor: `docs/generate_arhitektura.py`).

## Funkcionalnosti

- **Master-detail ekran** — `Lokacije`: zaglavlje (lokacija + voditelj iz FK padajuće liste) i detalji (`ZalihaSastojka` po lokaciji s FK na `Sastojak`).
- **Šifrarnik** — `Zaposlenici` (FK na `TipZaposlenika`).
- Navigacija i pretraživanje, CRUD na svim ekranima, padajuće liste za strane ključeve.
- **Složena validacija**: provjera HR OIB-a kontrolnom znamenkom (ISO 7064, MOD 11,10) — `ChainTrack.BLL/Validation/OibValidator.cs`.

## Pokretanje aplikacije

Potreban .NET 8 SDK (ili .NET 10 + `DOTNET_ROLL_FORWARD=LatestMajor`).

### Sa SQLite (bez vanjske baze, najlakše)

```bash
cd src/ChainTrack.Web
dotnet run -- --DatabaseProvider=Sqlite
```

### S PostgreSQL

Podigni Postgres na `localhost:5432` (user `postgres`, pass `postgres`, baza `chaintrack`), pa:

```bash
cd src/ChainTrack.Web
dotnet run
```

Baza se sama kreira i seed-a oglednim podacima (`DatabaseSeeder.SeedAsync`).
Aplikacija sluša na URL-u koji ispiše konzola (npr. `http://localhost:5xxx`).

## Pokretanje testova

```bash
dotnet test
```

Testovi koriste SQLite in-memory bazu (`SqliteTestContext`) i `WebApplicationFactory<Program>` za integracijske testove — **ponovljivi su**, mogu se pokretati više puta.

Pokriveni slojevi:
- **BLL jedinični:** `tests/.../Bll/` (OibValidator, LokacijaService, ZaposlenikService)
- **DAL jedinični:** `tests/.../Dal/` (Lokacija/Zaposlenik/ZalihaSastojka repozitoriji)
- **Prezentacijski jedinični:** `tests/.../Web/` (Lokacije/Zaposlenici kontroleri)
- **Integracijski:** preko `KontrolerTestPostavke` (cijeli stack na SQLite)
