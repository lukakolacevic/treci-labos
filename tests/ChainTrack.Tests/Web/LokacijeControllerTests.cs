using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Services;
using ChainTrack.Domain.Common;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using ChainTrack.Tests.TestSupport;
using ChainTrack.Web.Controllers;
using ChainTrack.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChainTrack.Tests.Web;

/// <summary>
/// JEDINIČNI TESTOVI PREZENTACIJSKOG SLOJA - <see cref="LokacijeController"/>
/// (složeni master-detail ekran). Poslovni sloj je zamijenjen lažnim objektom.
/// </summary>
public class LokacijeControllerTests
{
    private static Mock<ISifrarnikService> MockSifrarnik()
    {
        var sifrarnik = new Mock<ISifrarnikService>();
        sifrarnik.Setup(s => s.VoditeljiAsync()).ReturnsAsync(new List<Korisnik>());
        sifrarnik.Setup(s => s.SastojciAsync()).ReturnsAsync(new List<Sastojak>());
        return sifrarnik;
    }

    [Fact]
    public async Task Index_vracaViewSeStraniciranimRezultatom()
    {
        var lokServis = new Mock<ILokacijaService>();
        lokServis.Setup(s => s.PretraziAsync(null, 1, 20))
                 .ReturnsAsync(new PagedResult<Lokacija> { Stavke = new List<Lokacija>(), UkupnoZapisa = 0 });
        var kontroler = KontrolerTestPostavke.Pripremi(
            new LokacijeController(lokServis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Index(null, 1);

        var view = Assert.IsType<ViewResult>(rezultat);
        Assert.IsType<PagedResult<Lokacija>>(view.Model);
    }

    [Fact]
    public async Task Details_nepostojecaLokacija_vracaNotFound()
    {
        var lokServis = new Mock<ILokacijaService>();
        lokServis.Setup(s => s.DohvatiSDetaljimaAsync(99)).ReturnsAsync((Lokacija?)null);
        var kontroler = KontrolerTestPostavke.Pripremi(
            new LokacijeController(lokServis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Details(99);

        Assert.IsType<NotFoundResult>(rezultat);
    }

    [Fact]
    public async Task DodajZalihu_valjano_preusmjeravaNaDetails()
    {
        var lokServis = new Mock<ILokacijaService>();
        lokServis.Setup(s => s.DodajZalihuAsync(It.IsAny<ZalihaSastojka>()))
                 .ReturnsAsync(new ZalihaSastojka { Id = 1 });
        var kontroler = KontrolerTestPostavke.Pripremi(
            new LokacijeController(lokServis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.DodajZalihu(new ZalihaSastojkaViewModel
        {
            LokacijaId = 1, SastojakId = 5, Kolicina = 10m, MinimalnaRazina = 2m
        });

        var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
        Assert.Equal("Details", redirect.ActionName);
    }

    [Fact]
    public async Task DodajZalihu_validacijaException_vracaDetailsViewSPogreskom()
    {
        var lokServis = new Mock<ILokacijaService>();
        lokServis.Setup(s => s.DodajZalihuAsync(It.IsAny<ZalihaSastojka>()))
                 .ThrowsAsync(new ValidacijaException(
                     nameof(ZalihaSastojka.SastojakId), "Sastojak je već evidentiran."));
        lokServis.Setup(s => s.DohvatiSDetaljimaAsync(1))
                 .ReturnsAsync(new Lokacija
                 {
                     Id = 1, Naziv = "Lokacija", Adresa = "Adresa", Grad = "Grad",
                     TipObjekta = TipObjekta.RESTORAN
                 });
        var kontroler = KontrolerTestPostavke.Pripremi(
            new LokacijeController(lokServis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.DodajZalihu(new ZalihaSastojkaViewModel
        {
            LokacijaId = 1, SastojakId = 5, Kolicina = 10m, MinimalnaRazina = 2m
        });

        var view = Assert.IsType<ViewResult>(rezultat);
        Assert.Equal("Details", view.ViewName);
        Assert.False(kontroler.ModelState.IsValid);
        Assert.True(kontroler.ModelState.ContainsKey(nameof(ZalihaSastojka.SastojakId)));
    }

    [Fact]
    public async Task Obrisi_poslovnoPravilo_preusmjeravaNaDetailsSPorukom()
    {
        var lokServis = new Mock<ILokacijaService>();
        lokServis.Setup(s => s.ObrisiAsync(1))
                 .ThrowsAsync(new PoslovnoPraviloException("Lokacija ima aktivne zaposlenike."));
        var kontroler = KontrolerTestPostavke.Pripremi(
            new LokacijeController(lokServis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Obrisi(1);

        var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
        Assert.Equal("Details", redirect.ActionName);
        Assert.NotNull(kontroler.TempData["Greska"]);
    }
}
