using ChainTrack.Domain.Common;
using ChainTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.DAL.Repositories;

/// <summary>Pristup podacima za ekran šifrarnika zaposlenika.</summary>
public interface IZaposlenikRepository
{
    Task<PagedResult<Zaposlenik>> PretraziAsync(string? pojam, int? lokacijaId, int stranica, int velicinaStranice);
    Task<Zaposlenik?> DohvatiAsync(int id);
    Task<bool> PostojiAsync(int id);
    Task<Zaposlenik> DodajAsync(Zaposlenik zaposlenik);
    Task AzurirajAsync(Zaposlenik zaposlenik);
    Task ObrisiAsync(int id);

    /// <summary>Provjerava je li OIB već zauzet (podrška za pravilo jedinstvenosti).</summary>
    Task<bool> OibPostojiAsync(string oib, int? osimZaposlenikId = null);
}

/// <inheritdoc cref="IZaposlenikRepository"/>
public class ZaposlenikRepository : IZaposlenikRepository
{
    private readonly ChainTrackDbContext _db;

    public ZaposlenikRepository(ChainTrackDbContext db) => _db = db;

    public async Task<PagedResult<Zaposlenik>> PretraziAsync(
        string? pojam, int? lokacijaId, int stranica, int velicinaStranice)
    {
        if (stranica < 1) stranica = 1;
        if (velicinaStranice < 1) velicinaStranice = 20;

        IQueryable<Zaposlenik> upit = _db.Zaposlenici
            .AsNoTracking()
            .Include(z => z.Tip)
            .Include(z => z.Lokacija);

        if (!string.IsNullOrWhiteSpace(pojam))
        {
            string p = pojam.Trim().ToLower();
            upit = upit.Where(z =>
                z.Ime.ToLower().Contains(p) ||
                z.Prezime.ToLower().Contains(p) ||
                z.Oib.Contains(p));
        }

        if (lokacijaId is not null)
            upit = upit.Where(z => z.LokacijaId == lokacijaId);

        int ukupno = await upit.CountAsync();
        var stavke = await upit
            .OrderBy(z => z.Prezime).ThenBy(z => z.Ime)
            .Skip((stranica - 1) * velicinaStranice)
            .Take(velicinaStranice)
            .ToListAsync();

        return new PagedResult<Zaposlenik>
        {
            Stavke = stavke,
            UkupnoZapisa = ukupno,
            Stranica = stranica,
            VelicinaStranice = velicinaStranice
        };
    }

    public Task<Zaposlenik?> DohvatiAsync(int id) =>
        _db.Zaposlenici
            .Include(z => z.Tip)
            .Include(z => z.Lokacija)
            .FirstOrDefaultAsync(z => z.Id == id);

    public Task<bool> PostojiAsync(int id) => _db.Zaposlenici.AnyAsync(z => z.Id == id);

    public async Task<Zaposlenik> DodajAsync(Zaposlenik zaposlenik)
    {
        _db.Zaposlenici.Add(zaposlenik);
        await _db.SaveChangesAsync();
        return zaposlenik;
    }

    public async Task AzurirajAsync(Zaposlenik zaposlenik)
    {
        _db.Zaposlenici.Update(zaposlenik);
        await _db.SaveChangesAsync();
    }

    public async Task ObrisiAsync(int id)
    {
        var zaposlenik = await _db.Zaposlenici.FindAsync(id);
        if (zaposlenik is not null)
        {
            _db.Zaposlenici.Remove(zaposlenik);
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> OibPostojiAsync(string oib, int? osimZaposlenikId = null) =>
        _db.Zaposlenici.AnyAsync(z =>
            z.Oib == oib &&
            (osimZaposlenikId == null || z.Id != osimZaposlenikId));
}
