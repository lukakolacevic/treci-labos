using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Validation;
using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Common;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;

namespace ChainTrack.BLL.Services;

/// <summary>Poslovni sloj ekrana šifrarnika zaposlenika.</summary>
public interface IZaposlenikService
{
    Task<PagedResult<Zaposlenik>> PretraziAsync(string? pojam, int? lokacijaId, int stranica, int velicinaStranice);
    Task<Zaposlenik?> DohvatiAsync(int id);
    Task<Zaposlenik> KreirajAsync(Zaposlenik zaposlenik);
    Task AzurirajAsync(Zaposlenik zaposlenik);
    Task ObrisiAsync(int id);
    Task DeaktivirajAsync(int id);
}

/// <summary>
/// Poslovna pravila i validacija za zaposlenike. Provodi složenu validaciju
/// OIB-a (kontrolna znamenka) te poslovno pravilo iz dijagrama stanja (DZ1):
/// aktivnog zaposlenika nije dopušteno izravno obrisati.
/// </summary>
public class ZaposlenikService : IZaposlenikService
{
    private readonly IZaposlenikRepository _repo;

    public ZaposlenikService(IZaposlenikRepository repo) => _repo = repo;

    public Task<PagedResult<Zaposlenik>> PretraziAsync(
        string? pojam, int? lokacijaId, int stranica, int velicinaStranice) =>
        _repo.PretraziAsync(pojam, lokacijaId, stranica, velicinaStranice);

    public Task<Zaposlenik?> DohvatiAsync(int id) => _repo.DohvatiAsync(id);

    public async Task<Zaposlenik> KreirajAsync(Zaposlenik zaposlenik)
    {
        Ocisti(zaposlenik);
        await ValidirajAsync(zaposlenik, osimId: null);
        return await _repo.DodajAsync(zaposlenik);
    }

    public async Task AzurirajAsync(Zaposlenik zaposlenik)
    {
        if (!await _repo.PostojiAsync(zaposlenik.Id))
            throw new NijePronadenoException($"Zaposlenik #{zaposlenik.Id} ne postoji.");

        Ocisti(zaposlenik);
        await ValidirajAsync(zaposlenik, osimId: zaposlenik.Id);
        await _repo.AzurirajAsync(zaposlenik);
    }

    public async Task ObrisiAsync(int id)
    {
        var zaposlenik = await _repo.DohvatiAsync(id)
            ?? throw new NijePronadenoException($"Zaposlenik #{id} ne postoji.");

        // Poslovno pravilo (DZ1, poglavlje 6 - dijagram promjene stanja):
        // aktivnog zaposlenika nije dopušteno izravno obrisati.
        if (zaposlenik.Status == StatusZaposlenika.AKTIVAN)
            throw new PoslovnoPraviloException(
                "Aktivnog zaposlenika nije moguće obrisati. Prvo ga deaktivirajte.");

        await _repo.ObrisiAsync(id);
    }

    public async Task DeaktivirajAsync(int id)
    {
        var zaposlenik = await _repo.DohvatiAsync(id)
            ?? throw new NijePronadenoException($"Zaposlenik #{id} ne postoji.");

        zaposlenik.Status = StatusZaposlenika.NEAKTIVAN;
        await _repo.AzurirajAsync(zaposlenik);
    }

    private static void Ocisti(Zaposlenik z)
    {
        z.Ime = (z.Ime ?? string.Empty).Trim();
        z.Prezime = (z.Prezime ?? string.Empty).Trim();
        z.Oib = (z.Oib ?? string.Empty).Trim();
    }

    /// <summary>Cjelovita validacija zaposlenika prije spremanja.</summary>
    private async Task ValidirajAsync(Zaposlenik z, int? osimId)
    {
        var rezultat = new ValidacijaRezultat();

        if (string.IsNullOrWhiteSpace(z.Ime))
            rezultat.Dodaj(nameof(z.Ime), "Ime je obavezno.");

        if (string.IsNullOrWhiteSpace(z.Prezime))
            rezultat.Dodaj(nameof(z.Prezime), "Prezime je obavezno.");

        // SLOŽENA VALIDACIJA #1: provjera kontrolne znamenke OIB-a (ISO 7064, MOD 11,10).
        if (!OibValidator.JeValjan(z.Oib))
        {
            rezultat.Dodaj(nameof(z.Oib),
                "OIB nije ispravan - mora imati 11 znamenki i točnu kontrolnu znamenku.");
        }
        else if (await _repo.OibPostojiAsync(z.Oib, osimId))
        {
            // Pravilo jedinstvenosti - OIB ne smije biti dodijeljen dvama zaposlenicima.
            rezultat.Dodaj(nameof(z.Oib), "Zaposlenik s unesenim OIB-om već postoji.");
        }

        // Datum zaposlenja ne smije biti u budućnosti (FR-08).
        if (z.DatumZaposlenja > DateOnly.FromDateTime(DateTime.Today))
            rezultat.Dodaj(nameof(z.DatumZaposlenja), "Datum zaposlenja ne može biti u budućnosti.");

        if (z.Placa <= 0)
            rezultat.Dodaj(nameof(z.Placa), "Plaća mora biti veća od nule.");

        if (z.TipId <= 0)
            rezultat.Dodaj(nameof(z.TipId), "Odaberite tip zaposlenika.");

        if (z.LokacijaId <= 0)
            rezultat.Dodaj(nameof(z.LokacijaId), "Odaberite lokaciju.");

        rezultat.BaciAkoNijeValjano();
    }
}
