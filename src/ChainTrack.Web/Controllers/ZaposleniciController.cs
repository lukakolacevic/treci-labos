using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Services;
using ChainTrack.Domain.Entities;
using ChainTrack.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChainTrack.Web.Controllers;

/// <summary>
/// Prezentacijski sloj EKRANA ŠIFRARNIKA zaposlenika - navigacija, pretraživanje
/// i CRUD nad zaposlenicima, s padajućim listama za odabir stranih ključeva.
/// </summary>
public class ZaposleniciController : Controller
{
    private readonly IZaposlenikService _zaposlenikService;
    private readonly ISifrarnikService _sifrarnik;

    public ZaposleniciController(IZaposlenikService zaposlenikService, ISifrarnikService sifrarnik)
    {
        _zaposlenikService = zaposlenikService;
        _sifrarnik = sifrarnik;
    }

    // GET /Zaposlenici  - navigacija i pretraživanje
    public async Task<IActionResult> Index(string? pretraga, int? lokacijaId, int stranica = 1)
    {
        var rezultat = await _zaposlenikService.PretraziAsync(pretraga, lokacijaId, stranica, 20);

        ViewBag.Pretraga = pretraga;
        ViewBag.LokacijaId = lokacijaId;
        var lokacije = await _sifrarnik.LokacijeAsync();
        ViewBag.Lokacije = new SelectList(lokacije, nameof(Lokacija.Id), nameof(Lokacija.Naziv), lokacijaId);

        return View(rezultat);
    }

    // GET /Zaposlenici/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var zaposlenik = await _zaposlenikService.DohvatiAsync(id);
        return zaposlenik is null ? NotFound() : View(zaposlenik);
    }

    // GET /Zaposlenici/Create
    public async Task<IActionResult> Create()
    {
        await PripremiPadajuceListe();
        return View(new ZaposlenikViewModel());
    }

    // POST /Zaposlenici/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ZaposlenikViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var kreiran = await _zaposlenikService.KreirajAsync(UModel(model));
                TempData["Poruka"] = $"Zaposlenik {kreiran.PunoIme} je dodan.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidacijaException ex) { PreneseGreske(ex); }
        }

        await PripremiPadajuceListe();
        return View(model);
    }

    // GET /Zaposlenici/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var zaposlenik = await _zaposlenikService.DohvatiAsync(id);
        if (zaposlenik is null) return NotFound();

        await PripremiPadajuceListe();
        return View(UViewModel(zaposlenik));
    }

    // POST /Zaposlenici/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ZaposlenikViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _zaposlenikService.AzurirajAsync(UModel(model));
                TempData["Poruka"] = $"Zaposlenik {model.Ime} {model.Prezime} je ažuriran.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidacijaException ex) { PreneseGreske(ex); }
            catch (NijePronadenoException) { return NotFound(); }
        }

        await PripremiPadajuceListe();
        return View(model);
    }

    // POST /Zaposlenici/Obrisi  - brisanje (poslovno pravilo: aktivnog nije moguće obrisati)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        try
        {
            await _zaposlenikService.ObrisiAsync(id);
            TempData["Poruka"] = "Zaposlenik je obrisan.";
        }
        catch (PoslovnoPraviloException ex)
        {
            TempData["Greska"] = ex.Message;
        }
        catch (NijePronadenoException)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Index));
    }

    // POST /Zaposlenici/Deaktiviraj  - prijelaz stanja AKTIVAN -> NEAKTIVAN
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deaktiviraj(int id)
    {
        try
        {
            await _zaposlenikService.DeaktivirajAsync(id);
            TempData["Poruka"] = "Zaposlenik je deaktiviran.";
        }
        catch (NijePronadenoException)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Index));
    }

    // --- Pomoćne metode ---

    private static Zaposlenik UModel(ZaposlenikViewModel m) => new()
    {
        Id = m.Id,
        Ime = m.Ime,
        Prezime = m.Prezime,
        Oib = m.Oib,
        DatumZaposlenja = m.DatumZaposlenja,
        Placa = m.Placa,
        TipId = m.TipId,
        LokacijaId = m.LokacijaId,
        Status = m.Status
    };

    private static ZaposlenikViewModel UViewModel(Zaposlenik z) => new()
    {
        Id = z.Id,
        Ime = z.Ime,
        Prezime = z.Prezime,
        Oib = z.Oib,
        DatumZaposlenja = z.DatumZaposlenja,
        Placa = z.Placa,
        TipId = z.TipId,
        LokacijaId = z.LokacijaId,
        Status = z.Status
    };

    private async Task PripremiPadajuceListe()
    {
        var tipovi = await _sifrarnik.TipoviZaposlenikaAsync();
        var lokacije = await _sifrarnik.LokacijeAsync();
        ViewBag.Tipovi = new SelectList(tipovi, nameof(TipZaposlenika.Id), nameof(TipZaposlenika.Naziv));
        ViewBag.Lokacije = new SelectList(lokacije, nameof(Lokacija.Id), nameof(Lokacija.Naziv));
    }

    /// <summary>Preslikava pogreške validacije iz poslovnog sloja u ModelState.</summary>
    private void PreneseGreske(ValidacijaException ex)
    {
        foreach (var (polje, poruke) in ex.Greske)
            foreach (var poruka in poruke)
                ModelState.AddModelError(polje, poruka);
    }
}
