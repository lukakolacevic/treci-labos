using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Services;
using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using Moq;

namespace ChainTrack.Tests.Bll;

/// <summary>
/// JEDINIČNI TESTOVI POSLOVNOG SLOJA (BLL) - <see cref="LokacijaService"/>.
/// Repozitoriji su zamijenjeni lažnim objektima (Moq).
/// </summary>
public class LokacijaServiceTests
{
    private static (Mock<ILokacijaRepository>, Mock<IZalihaSastojkaRepository>) Mockovi()
        => (new Mock<ILokacijaRepository>(), new Mock<IZalihaSastojkaRepository>());

    [Fact]
    public async Task KreirajAsync_valjanaLokacija_pozivaRepozitorij()
    {
        var (lokRepo, zalRepo) = Mockovi();
        lokRepo.Setup(r => r.DodajAsync(It.IsAny<Lokacija>())).ReturnsAsync((Lokacija l) => l);
        var servis = new LokacijaService(lokRepo.Object, zalRepo.Object);

        await servis.KreirajAsync(new Lokacija
        {
            Naziv = "Nova", Adresa = "Adresa 1", Grad = "Zagreb", TipObjekta = TipObjekta.RESTORAN
        });

        lokRepo.Verify(r => r.DodajAsync(It.IsAny<Lokacija>()), Times.Once);
    }

    [Fact]
    public async Task KreirajAsync_prazanNaziv_bacaValidacijaException()
    {
        var (lokRepo, zalRepo) = Mockovi();
        var servis = new LokacijaService(lokRepo.Object, zalRepo.Object);

        var ex = await Assert.ThrowsAsync<ValidacijaException>(() => servis.KreirajAsync(
            new Lokacija { Naziv = "", Adresa = "Adresa", Grad = "Grad" }));

        Assert.True(ex.Greske.ContainsKey(nameof(Lokacija.Naziv)));
        lokRepo.Verify(r => r.DodajAsync(It.IsAny<Lokacija>()), Times.Never);
    }

    [Fact]
    public async Task DodajZalihuAsync_dvostrukiSastojak_bacaValidacijaException()
    {
        // Složeno poslovno pravilo: isti sastojak ne smije se ponoviti na lokaciji.
        var (lokRepo, zalRepo) = Mockovi();
        lokRepo.Setup(r => r.PostojiAsync(1)).ReturnsAsync(true);
        zalRepo.Setup(r => r.PostojiZaSastojakAsync(1, 5, null)).ReturnsAsync(true);
        var servis = new LokacijaService(lokRepo.Object, zalRepo.Object);

        var ex = await Assert.ThrowsAsync<ValidacijaException>(() => servis.DodajZalihuAsync(
            new ZalihaSastojka { LokacijaId = 1, SastojakId = 5, Kolicina = 10m, MinimalnaRazina = 2m }));

        Assert.True(ex.Greske.ContainsKey(nameof(ZalihaSastojka.SastojakId)));
        zalRepo.Verify(r => r.DodajAsync(It.IsAny<ZalihaSastojka>()), Times.Never);
    }

    [Fact]
    public async Task DodajZalihuAsync_jedinstveniSastojak_pozivaRepozitorij()
    {
        var (lokRepo, zalRepo) = Mockovi();
        lokRepo.Setup(r => r.PostojiAsync(1)).ReturnsAsync(true);
        zalRepo.Setup(r => r.PostojiZaSastojakAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
               .ReturnsAsync(false);
        zalRepo.Setup(r => r.DodajAsync(It.IsAny<ZalihaSastojka>())).ReturnsAsync((ZalihaSastojka z) => z);
        var servis = new LokacijaService(lokRepo.Object, zalRepo.Object);

        await servis.DodajZalihuAsync(
            new ZalihaSastojka { LokacijaId = 1, SastojakId = 5, Kolicina = 10m, MinimalnaRazina = 2m });

        zalRepo.Verify(r => r.DodajAsync(It.IsAny<ZalihaSastojka>()), Times.Once);
    }

    [Fact]
    public async Task ObrisiAsync_lokacijaSAktivnimZaposlenicima_bacaPoslovnoPraviloException()
    {
        var (lokRepo, zalRepo) = Mockovi();
        lokRepo.Setup(r => r.PostojiAsync(1)).ReturnsAsync(true);
        lokRepo.Setup(r => r.BrojAktivnihZaposlenikaAsync(1)).ReturnsAsync(3);
        var servis = new LokacijaService(lokRepo.Object, zalRepo.Object);

        await Assert.ThrowsAsync<PoslovnoPraviloException>(() => servis.ObrisiAsync(1));

        lokRepo.Verify(r => r.ObrisiAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObrisiAsync_lokacijaBezAktivnihZaposlenika_brise()
    {
        var (lokRepo, zalRepo) = Mockovi();
        lokRepo.Setup(r => r.PostojiAsync(1)).ReturnsAsync(true);
        lokRepo.Setup(r => r.BrojAktivnihZaposlenikaAsync(1)).ReturnsAsync(0);
        var servis = new LokacijaService(lokRepo.Object, zalRepo.Object);

        await servis.ObrisiAsync(1);

        lokRepo.Verify(r => r.ObrisiAsync(1), Times.Once);
    }
}
