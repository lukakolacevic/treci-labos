using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Services;
using ChainTrack.Domain.Entities;
using ChainTrack.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChainTrack.Web.Controllers;

/// <summary>
/// Prezentacijski sloj SLOŽENOG (master-detail) EKRANA.
/// Zaglavlje je lokacija, a detalji su stavke zalihe sastojaka na toj lokaciji.
/// Kontroler ne sadrži poslovnu logiku - delegira je poslovnom sloju (BLL).
/// </summary>
public class LokacijeController : Controller
{
    private readonly ILokacijaService _lokacijaService;
    private readonly ISifrarnikService _sifrarnik;

    public LokacijeController(ILokacijaService lokacijaService, ISifrarnikService sifrarnik)
    {
        _lokacijaService = lokacijaService;
        _sifrarnik = sifrarnik;
    }

    // GET /Lokacije  - navigacija i pretraživanje (master popis)
    public async Task<IActionResult> Index(string? pretraga, int stranica = 1)
    {
        var rezultat = await _lokacijaService.PretraziAsync(pretraga, stranica, 20);
        ViewBag.Pretraga = pretraga;
        return View(rezultat);
    }

    // GET /Lokacije/Create
    public async Task<IActionResult> Create()
    {
        await PripremiVoditelje();
        return View(new LokacijaViewModel());
    }

    // POST /Lokacije/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LokacijaViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var kreirana = await _lokacijaService.KreirajAsync(UModel(model));
                TempData["Poruka"] = $"Lokacija \"{kreirana.Naziv}\" je dodana.";
                return RedirectToAction(nameof(Details), new { id = kreirana.Id });
            }
            catch (ValidacijaException ex)
            {
                PreneseGreske(ex);
            }
        }

        await PripremiVoditelje();
        return View(model);
    }

    // GET /Lokacije/Details/5  - SLOŽENI EKRAN: zaglavlje + detalji
    public async Task<IActionResult> Details(int id)
    {
        var vm = await IzgradiDetaljeAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    // POST /Lokacije/SpremiZaglavlje  - spremanje zaglavlja složenog ekrana
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SpremiZaglavlje(LokacijaViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _lokacijaService.AzurirajAsync(UModel(model));
                TempData["Poruka"] = "Podaci o lokaciji su spremljeni.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (ValidacijaException ex) { PreneseGreske(ex); }
            catch (NijePronadenoException) { return NotFound(); }
        }

        var vm = await IzgradiDetaljeAsync(model.Id);
        if (vm is null) return NotFound();
        vm.Zaglavlje = model; // zadrži vrijednosti koje je korisnik unio
        return View(nameof(Details), vm);
    }

    // POST /Lokacije/Obrisi  - brisanje lokacije (poslovno pravilo u BLL-u)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        try
        {
            await _lokacijaService.ObrisiAsync(id);
            TempData["Poruka"] = "Lokacija je obrisana.";
            return RedirectToAction(nameof(Index));
        }
        catch (PoslovnoPraviloException ex)
        {
            TempData["Greska"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (NijePronadenoException)
        {
            return NotFound();
        }
    }

    // POST /Lokacije/DodajZalihu  - dodavanje retka DETALJA složenog ekrana
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DodajZalihu(ZalihaSastojkaViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _lokacijaService.DodajZalihuAsync(UModelZaliha(model));
                TempData["Poruka"] = "Stavka zalihe je dodana.";
                return RedirectToAction(nameof(Details), new { id = model.LokacijaId });
            }
            catch (ValidacijaException ex) { PreneseGreske(ex); }
            catch (PoslovnoPraviloException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (NijePronadenoException) { return NotFound(); }
        }

        var vm = await IzgradiDetaljeAsync(model.LokacijaId);
        if (vm is null) return NotFound();
        vm.NovaZaliha = model;
        return View(nameof(Details), vm);
    }

    // GET /Lokacije/UrediZalihu/5  - obrazac za uređivanje retka detalja
    public async Task<IActionResult> UrediZalihu(int id)
    {
        var zaliha = await _lokacijaService.DohvatiZalihuAsync(id);
        if (zaliha is null) return NotFound();

        await PripremiSastojke();
        ViewBag.NazivLokacije = zaliha.Lokacija?.Naziv;
        return View(new ZalihaSastojkaViewModel
        {
            Id = zaliha.Id,
            LokacijaId = zaliha.LokacijaId,
            SastojakId = zaliha.SastojakId,
            Kolicina = zaliha.Kolicina,
            MinimalnaRazina = zaliha.MinimalnaRazina
        });
    }

    // POST /Lokacije/UrediZalihu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UrediZalihu(ZalihaSastojkaViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _lokacijaService.AzurirajZalihuAsync(UModelZaliha(model));
                TempData["Poruka"] = "Stavka zalihe je ažurirana.";
                return RedirectToAction(nameof(Details), new { id = model.LokacijaId });
            }
            catch (ValidacijaException ex) { PreneseGreske(ex); }
            catch (NijePronadenoException) { return NotFound(); }
        }

        await PripremiSastojke();
        return View(model);
    }

    // POST /Lokacije/ObrisiZalihu  - brisanje retka detalja
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ObrisiZalihu(int id, int lokacijaId)
    {
        await _lokacijaService.ObrisiZalihuAsync(id);
        TempData["Poruka"] = "Stavka zalihe je obrisana.";
        return RedirectToAction(nameof(Details), new { id = lokacijaId });
    }

    // --- Pomoćne metode ---

    private async Task<LokacijaDetaljiViewModel?> IzgradiDetaljeAsync(int id)
    {
        var lokacija = await _lokacijaService.DohvatiSDetaljimaAsync(id);
        if (lokacija is null) return null;

        await PripremiVoditelje();
        await PripremiSastojke();

        return new LokacijaDetaljiViewModel
        {
            Zaglavlje = new LokacijaViewModel
            {
                Id = lokacija.Id,
                Naziv = lokacija.Naziv,
                Adresa = lokacija.Adresa,
                Grad = lokacija.Grad,
                TipObjekta = lokacija.TipObjekta,
                Aktivna = lokacija.Aktivna,
                VoditeljId = lokacija.VoditeljId
            },
            Zalihe = lokacija.Zalihe.OrderBy(z => z.Sastojak!.Naziv).ToList(),
            NovaZaliha = new ZalihaSastojkaViewModel { LokacijaId = id }
        };
    }

    private static Lokacija UModel(LokacijaViewModel m) => new()
    {
        Id = m.Id,
        Naziv = m.Naziv,
        Adresa = m.Adresa,
        Grad = m.Grad,
        TipObjekta = m.TipObjekta,
        Aktivna = m.Aktivna,
        VoditeljId = m.VoditeljId
    };

    private static ZalihaSastojka UModelZaliha(ZalihaSastojkaViewModel m) => new()
    {
        Id = m.Id,
        LokacijaId = m.LokacijaId,
        SastojakId = m.SastojakId,
        Kolicina = m.Kolicina,
        MinimalnaRazina = m.MinimalnaRazina
    };

    private async Task PripremiVoditelje()
    {
        var voditelji = await _sifrarnik.VoditeljiAsync();
        ViewBag.Voditelji = new SelectList(voditelji, nameof(Korisnik.Id), nameof(Korisnik.PunoIme));
    }

    private async Task PripremiSastojke()
    {
        var sastojci = await _sifrarnik.SastojciAsync();
        ViewBag.Sastojci = new SelectList(sastojci, nameof(Sastojak.Id), nameof(Sastojak.Naziv));
    }

    /// <summary>Preslikava pogreške validacije iz poslovnog sloja u ModelState.</summary>
    private void PreneseGreske(ValidacijaException ex)
    {
        foreach (var (polje, poruke) in ex.Greske)
            foreach (var poruka in poruke)
                ModelState.AddModelError(polje, poruka);
    }
}
