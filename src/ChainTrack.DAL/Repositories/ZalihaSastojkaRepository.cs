using ChainTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.DAL.Repositories;

/// <summary>Pristup podacima za retke detalja složenog ekrana (zaliha sastojka).</summary>
public interface IZalihaSastojkaRepository
{
    Task<ZalihaSastojka?> DohvatiAsync(int id);
    Task<bool> PostojiAsync(int id);
    Task<ZalihaSastojka> DodajAsync(ZalihaSastojka zaliha);
    Task AzurirajAsync(ZalihaSastojka zaliha);
    Task ObrisiAsync(int id);

    /// <summary>
    /// Provjerava postoji li već zaliha za zadani sastojak na zadanoj lokaciji.
    /// Podrška za poslovno pravilo: isti sastojak ne smije se ponoviti na lokaciji.
    /// </summary>
    Task<bool> PostojiZaSastojakAsync(int lokacijaId, int sastojakId, int? osimZalihaId = null);
}

/// <inheritdoc cref="IZalihaSastojkaRepository"/>
public class ZalihaSastojkaRepository : IZalihaSastojkaRepository
{
    private readonly ChainTrackDbContext _db;

    public ZalihaSastojkaRepository(ChainTrackDbContext db) => _db = db;

    public Task<ZalihaSastojka?> DohvatiAsync(int id) =>
        _db.ZaliheSastojaka
            .Include(z => z.Sastojak)
            .Include(z => z.Lokacija)
            .FirstOrDefaultAsync(z => z.Id == id);

    public Task<bool> PostojiAsync(int id) => _db.ZaliheSastojaka.AnyAsync(z => z.Id == id);

    public async Task<ZalihaSastojka> DodajAsync(ZalihaSastojka zaliha)
    {
        _db.ZaliheSastojaka.Add(zaliha);
        await _db.SaveChangesAsync();
        return zaliha;
    }

    public async Task AzurirajAsync(ZalihaSastojka zaliha)
    {
        _db.ZaliheSastojaka.Update(zaliha);
        await _db.SaveChangesAsync();
    }

    public async Task ObrisiAsync(int id)
    {
        var zaliha = await _db.ZaliheSastojaka.FindAsync(id);
        if (zaliha is not null)
        {
            _db.ZaliheSastojaka.Remove(zaliha);
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> PostojiZaSastojakAsync(int lokacijaId, int sastojakId, int? osimZalihaId = null) =>
        _db.ZaliheSastojaka.AnyAsync(z =>
            z.LokacijaId == lokacijaId &&
            z.SastojakId == sastojakId &&
            (osimZalihaId == null || z.Id != osimZalihaId));
}
