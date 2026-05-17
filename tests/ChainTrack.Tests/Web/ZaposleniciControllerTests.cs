using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Services;
using ChainTrack.Domain.Common;
using ChainTrack.Domain.Entities;
using ChainTrack.Tests.TestSupport;
using ChainTrack.Web.Controllers;
using ChainTrack.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChainTrack.Tests.Web;

/// <summary>
/// JEDINIČNI TESTOVI PREZENTACIJSKOG SLOJA - <see cref="ZaposleniciController"/>.
/// Poslovni sloj je zamijenjen lažnim objektom (Moq), pa se testira ponašanje
/// kontrolera (vrsta rezultata, preusmjeravanje, ModelState).
/// </summary>
public class ZaposleniciControllerTests
{
    private static Mock<ISifrarnikService> MockSifrarnik()
    {
        var sifrarnik = new Mock<ISifrarnikService>();
        sifrarnik.Setup(s => s.TipoviZaposlenikaAsync()).ReturnsAsync(new List<TipZaposlenika>());
        sifrarnik.Setup(s => s.LokacijeAsync()).ReturnsAsync(new List<Lokacija>());
        return sifrarnik;
    }

    private static ZaposlenikViewModel ValjaniViewModel() => new()
    {
        Ime = "Ivan", Prezime = "Horvat", Oib = "12345678903",
        DatumZaposlenja = new DateOnly(2024, 1, 1), Placa = 1200m,
        TipId = 1, LokacijaId = 1
    };

    [Fact]
    public async Task Create_POST_valjanModel_preusmjeravaNaIndex()
    {
        var servis = new Mock<IZaposlenikService>();
        servis.Setup(s => s.KreirajAsync(It.IsAny<Zaposlenik>()))
              .ReturnsAsync(new Zaposlenik { Id = 1, Ime = "Ivan", Prezime = "Horvat" });
        var kontroler = KontrolerTestPostavke.Pripremi(
            new ZaposleniciController(servis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Create(ValjaniViewModel());

        var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
        Assert.Equal("Index", redirect.ActionName);
        servis.Verify(s => s.KreirajAsync(It.IsAny<Zaposlenik>()), Times.Once);
    }

    [Fact]
    public async Task Create_POST_validacijaException_vracaViewSPogreskama()
    {
        var servis = new Mock<IZaposlenikService>();
        servis.Setup(s => s.KreirajAsync(It.IsAny<Zaposlenik>()))
              .ThrowsAsync(new ValidacijaException(nameof(Zaposlenik.Oib), "OIB nije ispravan."));
        var kontroler = KontrolerTestPostavke.Pripremi(
            new ZaposleniciController(servis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Create(ValjaniViewModel());

        Assert.IsType<ViewResult>(rezultat);
        Assert.False(kontroler.ModelState.IsValid);
        Assert.True(kontroler.ModelState.ContainsKey(nameof(Zaposlenik.Oib)));
    }

    [Fact]
    public async Task Create_POST_nevaljanModelState_neDelegiraServisu()
    {
        var servis = new Mock<IZaposlenikService>();
        var kontroler = KontrolerTestPostavke.Pripremi(
            new ZaposleniciController(servis.Object, MockSifrarnik().Object));
        kontroler.ModelState.AddModelError(nameof(ZaposlenikViewModel.Ime), "Ime je obavezno.");

        var rezultat = await kontroler.Create(ValjaniViewModel());

        Assert.IsType<ViewResult>(rezultat);
        servis.Verify(s => s.KreirajAsync(It.IsAny<Zaposlenik>()), Times.Never);
    }

    [Fact]
    public async Task Index_vracaViewSeStraniciranimRezultatom()
    {
        var servis = new Mock<IZaposlenikService>();
        servis.Setup(s => s.PretraziAsync(null, null, 1, 20))
              .ReturnsAsync(new PagedResult<Zaposlenik>
              {
                  Stavke = new List<Zaposlenik>(), UkupnoZapisa = 0, Stranica = 1
              });
        var kontroler = KontrolerTestPostavke.Pripremi(
            new ZaposleniciController(servis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Index(null, null, 1);

        var view = Assert.IsType<ViewResult>(rezultat);
        Assert.IsType<PagedResult<Zaposlenik>>(view.Model);
    }

    [Fact]
    public async Task Details_nepostojeciZaposlenik_vracaNotFound()
    {
        var servis = new Mock<IZaposlenikService>();
        servis.Setup(s => s.DohvatiAsync(99)).ReturnsAsync((Zaposlenik?)null);
        var kontroler = KontrolerTestPostavke.Pripremi(
            new ZaposleniciController(servis.Object, MockSifrarnik().Object));

        var rezultat = await kontroler.Details(99);

        Assert.IsType<NotFoundResult>(rezultat);
    }
}
