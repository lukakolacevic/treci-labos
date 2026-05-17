using ChainTrack.BLL.Exceptions;
using ChainTrack.BLL.Services;
using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using Moq;

namespace ChainTrack.Tests.Bll;

/// <summary>
/// JEDINIČNI TESTOVI POSLOVNOG SLOJA (BLL) - <see cref="ZaposlenikService"/>.
/// Sloj za pristup podacima je zamijenjen lažnim objektom (Moq), pa se testira
/// isključivo poslovna logika i validacija.
/// </summary>
public class ZaposlenikServiceTests
{
    private static Zaposlenik ValjaniZaposlenik() => new()
    {
        Ime = "Ivan", Prezime = "Horvat", Oib = "12345678903",
        DatumZaposlenja = new DateOnly(2024, 1, 1), Placa = 1200m,
        TipId = 1, LokacijaId = 1, Status = StatusZaposlenika.AKTIVAN
    };

    [Fact]
    public async Task KreirajAsync_valjaniPodaci_pozivaRepozitorij()
    {
        var repo = new Mock<IZaposlenikRepository>();
        repo.Setup(r => r.OibPostojiAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
        repo.Setup(r => r.DodajAsync(It.IsAny<Zaposlenik>())).ReturnsAsync((Zaposlenik z) => z);
        var servis = new ZaposlenikService(repo.Object);

        await servis.KreirajAsync(ValjaniZaposlenik());

        repo.Verify(r => r.DodajAsync(It.IsAny<Zaposlenik>()), Times.Once);
    }

    [Fact]
    public async Task KreirajAsync_neispravanOib_bacaValidacijaException()
    {
        var repo = new Mock<IZaposlenikRepository>();
        repo.Setup(r => r.OibPostojiAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
        var servis = new ZaposlenikService(repo.Object);

        var zaposlenik = ValjaniZaposlenik();
        zaposlenik.Oib = "12345678900"; // pogrešna kontrolna znamenka

        var ex = await Assert.ThrowsAsync<ValidacijaException>(() => servis.KreirajAsync(zaposlenik));

        Assert.True(ex.Greske.ContainsKey(nameof(Zaposlenik.Oib)));
        repo.Verify(r => r.DodajAsync(It.IsAny<Zaposlenik>()), Times.Never);
    }

    [Fact]
    public async Task KreirajAsync_vecZauzetOib_bacaValidacijaException()
    {
        var repo = new Mock<IZaposlenikRepository>();
        repo.Setup(r => r.OibPostojiAsync("12345678903", It.IsAny<int?>())).ReturnsAsync(true);
        var servis = new ZaposlenikService(repo.Object);

        var ex = await Assert.ThrowsAsync<ValidacijaException>(() => servis.KreirajAsync(ValjaniZaposlenik()));

        Assert.True(ex.Greske.ContainsKey(nameof(Zaposlenik.Oib)));
        repo.Verify(r => r.DodajAsync(It.IsAny<Zaposlenik>()), Times.Never);
    }

    [Fact]
    public async Task KreirajAsync_datumZaposlenjaUBuducnosti_bacaValidacijaException()
    {
        var repo = new Mock<IZaposlenikRepository>();
        repo.Setup(r => r.OibPostojiAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
        var servis = new ZaposlenikService(repo.Object);

        var zaposlenik = ValjaniZaposlenik();
        zaposlenik.DatumZaposlenja = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var ex = await Assert.ThrowsAsync<ValidacijaException>(() => servis.KreirajAsync(zaposlenik));

        Assert.True(ex.Greske.ContainsKey(nameof(Zaposlenik.DatumZaposlenja)));
    }

    [Fact]
    public async Task ObrisiAsync_aktivnogZaposlenika_bacaPoslovnoPraviloException()
    {
        var repo = new Mock<IZaposlenikRepository>();
        repo.Setup(r => r.DohvatiAsync(1)).ReturnsAsync(new Zaposlenik
        {
            Id = 1, Oib = "12345678903", Status = StatusZaposlenika.AKTIVAN
        });
        var servis = new ZaposlenikService(repo.Object);

        await Assert.ThrowsAsync<PoslovnoPraviloException>(() => servis.ObrisiAsync(1));

        repo.Verify(r => r.ObrisiAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObrisiAsync_neaktivnogZaposlenika_brise()
    {
        var repo = new Mock<IZaposlenikRepository>();
        repo.Setup(r => r.DohvatiAsync(1)).ReturnsAsync(new Zaposlenik
        {
            Id = 1, Oib = "12345678903", Status = StatusZaposlenika.NEAKTIVAN
        });
        var servis = new ZaposlenikService(repo.Object);

        await servis.ObrisiAsync(1);

        repo.Verify(r => r.ObrisiAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeaktivirajAsync_postavljaStatusNaNeaktivan()
    {
        var repo = new Mock<IZaposlenikRepository>();
        var zaposlenik = new Zaposlenik { Id = 1, Oib = "12345678903", Status = StatusZaposlenika.AKTIVAN };
        repo.Setup(r => r.DohvatiAsync(1)).ReturnsAsync(zaposlenik);
        var servis = new ZaposlenikService(repo.Object);

        await servis.DeaktivirajAsync(1);

        Assert.Equal(StatusZaposlenika.NEAKTIVAN, zaposlenik.Status);
        repo.Verify(r => r.AzurirajAsync(It.IsAny<Zaposlenik>()), Times.Once);
    }
}
