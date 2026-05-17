using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Validation;
using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Common;
using ChainTrack.Domain.Entities;

namespace ChainTrack.BLL.Services;

/// <summary>
/// Poslovni sloj složenog (master-detail) ekrana: lokacija kao zaglavlje i
/// zalihe sastojaka kao detalji.
/// </summary>
public interface ILokacijaService
{
    // --- Zaglavlje (master) ---
    Task<PagedResult<Lokacija>> PretraziAsync(string? pojam, int stranica, int velicinaStranice);
    Task<Lokacija?> DohvatiAsync(int id);
    Task<Lokacija?> DohvatiSDetaljimaAsync(int id);
    Task<Lokacija> KreirajAsync(Lokacija lokacija);
    Task AzurirajAsync(Lokacija lokacija);
    Task ObrisiAsync(int id);

    // --- Detalji (zalihe sastojaka) ---
    Task<ZalihaSastojka?> DohvatiZalihuAsync(int zalihaId);
    Task<ZalihaSastojka> DodajZalihuAsync(ZalihaSastojka zaliha);
    Task AzurirajZalihuAsync(ZalihaSastojka zaliha);
    Task ObrisiZalihuAsync(int zalihaId);
}

/// <inheritdoc cref="ILokacijaService"/>
public class LokacijaService : ILokacijaService
{
    private readonly ILokacijaRepository _lokacijaRepo;
    private readonly IZalihaSastojkaRepository _zalihaRepo;

    public LokacijaService(ILokacijaRepository lokacijaRepo, IZalihaSastojkaRepository zalihaRepo)
    {
        _lokacijaRepo = lokacijaRepo;
        _zalihaRepo = zalihaRepo;
    }

    public Task<PagedResult<Lokacija>> PretraziAsync(string? pojam, int stranica, int velicinaStranice) =>
        _lokacijaRepo.PretraziAsync(pojam, stranica, velicinaStranice);

    public Task<Lokacija?> DohvatiAsync(int id) => _lokacijaRepo.DohvatiAsync(id);

    public Task<Lokacija?> DohvatiSDetaljimaAsync(int id) => _lokacijaRepo.DohvatiSDetaljimaAsync(id);

    public async Task<Lokacija> KreirajAsync(Lokacija lokacija)
    {
        ValidirajZaglavlje(lokacija);
        return await _lokacijaRepo.DodajAsync(lokacija);
    }

    public async Task AzurirajAsync(Lokacija lokacija)
    {
        if (!await _lokacijaRepo.PostojiAsync(lokacija.Id))
            throw new NijePronadenoException($"Lokacija #{lokacija.Id} ne postoji.");

        ValidirajZaglavlje(lokacija);
        await _lokacijaRepo.AzurirajAsync(lokacija);
    }

    public async Task ObrisiAsync(int id)
    {
        if (!await _lokacijaRepo.PostojiAsync(id))
            throw new NijePronadenoException($"Lokacija #{id} ne postoji.");

        // Poslovno pravilo (DZ1, FR-01): lokaciju s aktivnim zaposlenicima
        // nije dopušteno obrisati.
        int aktivnih = await _lokacijaRepo.BrojAktivnihZaposlenikaAsync(id);
        if (aktivnih > 0)
            throw new PoslovnoPraviloException(
                $"Lokaciju nije moguće obrisati jer ima {aktivnih} aktivnih zaposlenika.");

        await _lokacijaRepo.ObrisiAsync(id);
    }

    public Task<ZalihaSastojka?> DohvatiZalihuAsync(int zalihaId) => _zalihaRepo.DohvatiAsync(zalihaId);

    public async Task<ZalihaSastojka> DodajZalihuAsync(ZalihaSastojka zaliha)
    {
        if (!await _lokacijaRepo.PostojiAsync(zaliha.LokacijaId))
            throw new NijePronadenoException($"Lokacija #{zaliha.LokacijaId} ne postoji.");

        await ValidirajDetaljAsync(zaliha, osimZalihaId: null);
        zaliha.Azurirano = DateTime.UtcNow;
        return await _zalihaRepo.DodajAsync(zaliha);
    }

    public async Task AzurirajZalihuAsync(ZalihaSastojka zaliha)
    {
        if (!await _zalihaRepo.PostojiAsync(zaliha.Id))
            throw new NijePronadenoException($"Stavka zalihe #{zaliha.Id} ne postoji.");

        await ValidirajDetaljAsync(zaliha, osimZalihaId: zaliha.Id);
        zaliha.Azurirano = DateTime.UtcNow;
        await _zalihaRepo.AzurirajAsync(zaliha);
    }

    public Task ObrisiZalihuAsync(int zalihaId) => _zalihaRepo.ObrisiAsync(zalihaId);

    private static void ValidirajZaglavlje(Lokacija l)
    {
        var rezultat = new ValidacijaRezultat();

        if (string.IsNullOrWhiteSpace(l.Naziv))
            rezultat.Dodaj(nameof(l.Naziv), "Naziv lokacije je obavezan.");
        if (string.IsNullOrWhiteSpace(l.Adresa))
            rezultat.Dodaj(nameof(l.Adresa), "Adresa je obavezna.");
        if (string.IsNullOrWhiteSpace(l.Grad))
            rezultat.Dodaj(nameof(l.Grad), "Grad je obavezan.");

        rezultat.BaciAkoNijeValjano();
    }

    /// <summary>Validacija retka detalja (stavke zalihe).</summary>
    private async Task ValidirajDetaljAsync(ZalihaSastojka z, int? osimZalihaId)
    {
        var rezultat = new ValidacijaRezultat();

        if (z.SastojakId <= 0)
            rezultat.Dodaj(nameof(z.SastojakId), "Odaberite sastojak.");

        if (z.Kolicina < 0)
            rezultat.Dodaj(nameof(z.Kolicina), "Količina ne može biti negativna.");

        if (z.MinimalnaRazina < 0)
            rezultat.Dodaj(nameof(z.MinimalnaRazina), "Minimalna razina ne može biti negativna.");

        // SLOŽENA VALIDACIJA #2: isti sastojak ne smije se ponoviti na istoj lokaciji.
        // Pravilo se ne svodi na "popunjeno / u rasponu" - provjerava se jedinstvenost
        // para (lokacija, sastojak) među svim postojećim stavkama.
        if (z.SastojakId > 0 &&
            await _zalihaRepo.PostojiZaSastojakAsync(z.LokacijaId, z.SastojakId, osimZalihaId))
        {
            rezultat.Dodaj(nameof(z.SastojakId),
                "Taj sastojak je već evidentiran na ovoj lokaciji - uredite postojeću stavku.");
        }

        rezultat.BaciAkoNijeValjano();
    }
}
