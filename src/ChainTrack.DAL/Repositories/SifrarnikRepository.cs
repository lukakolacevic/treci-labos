using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.DAL.Repositories;

/// <summary>
/// Pristup šifrarničkim (lookup) podacima koji pune padajuće liste za odabir
/// stranog ključa na ekranima.
/// </summary>
public interface ISifrarnikRepository
{
    Task<IReadOnlyList<TipZaposlenika>> TipoviZaposlenikaAsync();
    Task<IReadOnlyList<Lokacija>> LokacijeAsync();
    Task<IReadOnlyList<Sastojak>> SastojciAsync();
    Task<IReadOnlyList<Korisnik>> VoditeljiAsync();
}

/// <inheritdoc cref="ISifrarnikRepository"/>
public class SifrarnikRepository : ISifrarnikRepository
{
    private readonly ChainTrackDbContext _db;

    public SifrarnikRepository(ChainTrackDbContext db) => _db = db;

    public async Task<IReadOnlyList<TipZaposlenika>> TipoviZaposlenikaAsync() =>
        await _db.TipoviZaposlenika.AsNoTracking().OrderBy(t => t.Naziv).ToListAsync();

    public async Task<IReadOnlyList<Lokacija>> LokacijeAsync() =>
        await _db.Lokacije.AsNoTracking().OrderBy(l => l.Naziv).ToListAsync();

    public async Task<IReadOnlyList<Sastojak>> SastojciAsync() =>
        await _db.Sastojci.AsNoTracking()
            .Include(s => s.Kategorija)
            .OrderBy(s => s.Naziv)
            .ToListAsync();

    public async Task<IReadOnlyList<Korisnik>> VoditeljiAsync() =>
        await _db.Korisnici.AsNoTracking()
            .Where(k => k.Aktivan && k.Uloga == UlogaKorisnika.VODITELJ_LOKACIJE)
            .OrderBy(k => k.Prezime).ThenBy(k => k.Ime)
            .ToListAsync();
}
