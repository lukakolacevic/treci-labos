using ChainTrack.Domain.Common;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.DAL.Repositories;

/// <summary>Pristup podacima za zaglavlje složenog ekrana (lokacija).</summary>
public interface ILokacijaRepository
{
    Task<PagedResult<Lokacija>> PretraziAsync(string? pojam, int stranica, int velicinaStranice);
    Task<Lokacija?> DohvatiAsync(int id);
    Task<Lokacija?> DohvatiSDetaljimaAsync(int id);
    Task<Lokacija> DodajAsync(Lokacija lokacija);
    Task AzurirajAsync(Lokacija lokacija);
    Task ObrisiAsync(int id);
    Task<bool> PostojiAsync(int id);
    Task<int> BrojAktivnihZaposlenikaAsync(int lokacijaId);
}

/// <inheritdoc cref="ILokacijaRepository"/>
public class LokacijaRepository : ILokacijaRepository
{
    private readonly ChainTrackDbContext _db;

    public LokacijaRepository(ChainTrackDbContext db) => _db = db;

    public async Task<PagedResult<Lokacija>> PretraziAsync(string? pojam, int stranica, int velicinaStranice)
    {
        if (stranica < 1) stranica = 1;
        if (velicinaStranice < 1) velicinaStranice = 20;

        IQueryable<Lokacija> upit = _db.Lokacije.AsNoTracking().Include(l => l.Voditelj);

        if (!string.IsNullOrWhiteSpace(pojam))
        {
            string p = pojam.Trim().ToLower();
            upit = upit.Where(l =>
                l.Naziv.ToLower().Contains(p) ||
                l.Grad.ToLower().Contains(p) ||
                l.Adresa.ToLower().Contains(p));
        }

        int ukupno = await upit.CountAsync();
        var stavke = await upit
            .OrderBy(l => l.Naziv)
            .Skip((stranica - 1) * velicinaStranice)
            .Take(velicinaStranice)
            .ToListAsync();

        return new PagedResult<Lokacija>
        {
            Stavke = stavke,
            UkupnoZapisa = ukupno,
            Stranica = stranica,
            VelicinaStranice = velicinaStranice
        };
    }

    public Task<Lokacija?> DohvatiAsync(int id) =>
        _db.Lokacije.FirstOrDefaultAsync(l => l.Id == id);

    public Task<Lokacija?> DohvatiSDetaljimaAsync(int id) =>
        _db.Lokacije
            .Include(l => l.Voditelj)
            .Include(l => l.Zalihe).ThenInclude(z => z.Sastojak)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Lokacija> DodajAsync(Lokacija lokacija)
    {
        _db.Lokacije.Add(lokacija);
        await _db.SaveChangesAsync();
        return lokacija;
    }

    public async Task AzurirajAsync(Lokacija lokacija)
    {
        _db.Lokacije.Update(lokacija);
        await _db.SaveChangesAsync();
    }

    public async Task ObrisiAsync(int id)
    {
        var lokacija = await _db.Lokacije.FindAsync(id);
        if (lokacija is not null)
        {
            _db.Lokacije.Remove(lokacija);
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> PostojiAsync(int id) => _db.Lokacije.AnyAsync(l => l.Id == id);

    public Task<int> BrojAktivnihZaposlenikaAsync(int lokacijaId) =>
        _db.Zaposlenici.CountAsync(z =>
            z.LokacijaId == lokacijaId && z.Status == StatusZaposlenika.AKTIVAN);
}
