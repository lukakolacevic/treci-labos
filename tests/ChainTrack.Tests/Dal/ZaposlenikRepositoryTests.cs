using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using ChainTrack.Tests.TestSupport;

namespace ChainTrack.Tests.Dal;

/// <summary>JEDINIČNI TESTOVI SLOJA ZA PRISTUP PODACIMA (DAL) - zaposlenici.</summary>
public class ZaposlenikRepositoryTests
{
    private static Zaposlenik Noviovi(OglednoOkruzenje o, string oib, string prezime,
        StatusZaposlenika status = StatusZaposlenika.AKTIVAN) => new()
    {
        Ime = "Test", Prezime = prezime, Oib = oib,
        DatumZaposlenja = new DateOnly(2024, 1, 1), Placa = 1000m,
        TipId = o.Tip.Id, LokacijaId = o.Lokacija.Id, Status = status
    };

    [Fact]
    public async Task DodajAsync_trajnoSpremaZaposlenika()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);

        var repo = new ZaposlenikRepository(db.NoviKontekst());
        var zaposlenik = Noviovi(okruzenje, "12345678903", "Horvat");
        await repo.DodajAsync(zaposlenik);

        var provjera = new ZaposlenikRepository(db.NoviKontekst());
        var dohvacen = await provjera.DohvatiAsync(zaposlenik.Id);

        Assert.NotNull(dohvacen);
        Assert.Equal("Horvat", dohvacen!.Prezime);
        Assert.Equal("Kuhar", dohvacen.Tip!.Naziv);
    }

    [Fact]
    public async Task OibPostojiAsync_vracaTrueZaPostojeciOib()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        using (var ctx = db.NoviKontekst())
        {
            ctx.Zaposlenici.Add(Noviovi(okruzenje, "12345678903", "Postojeci"));
            ctx.SaveChanges();
        }
        var repo = new ZaposlenikRepository(db.NoviKontekst());

        Assert.True(await repo.OibPostojiAsync("12345678903"));
        Assert.False(await repo.OibPostojiAsync("26039951106"));
    }

    [Fact]
    public async Task OibPostojiAsync_iskljucujeZadaniZaposlenik()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        Zaposlenik postojeci;
        using (var ctx = db.NoviKontekst())
        {
            postojeci = Noviovi(okruzenje, "12345678903", "Postojeci");
            ctx.Zaposlenici.Add(postojeci);
            ctx.SaveChanges();
        }
        var repo = new ZaposlenikRepository(db.NoviKontekst());

        // Kod uređivanja istog zaposlenika njegov OIB ne smije se tretirati kao zauzet.
        Assert.False(await repo.OibPostojiAsync("12345678903", osimZaposlenikId: postojeci.Id));
    }

    [Fact]
    public async Task PretraziAsync_filtriraPoPrezimenuIFiltriraPoLokaciji()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        using (var ctx = db.NoviKontekst())
        {
            ctx.Zaposlenici.AddRange(
                Noviovi(okruzenje, "12345678903", "Horvat"),
                Noviovi(okruzenje, "26039951106", "Kovac"),
                Noviovi(okruzenje, "65120987306", "Horvatic"));
            ctx.SaveChanges();
        }
        var repo = new ZaposlenikRepository(db.NoviKontekst());

        var rezultat = await repo.PretraziAsync("horvat", null, 1, 20);

        Assert.Equal(2, rezultat.UkupnoZapisa);
    }

    [Fact]
    public async Task PostojiAsync_vracaTocnoStanje()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        Zaposlenik zaposlenik;
        using (var ctx = db.NoviKontekst())
        {
            zaposlenik = Noviovi(okruzenje, "12345678903", "Test");
            ctx.Zaposlenici.Add(zaposlenik);
            ctx.SaveChanges();
        }
        var repo = new ZaposlenikRepository(db.NoviKontekst());

        Assert.True(await repo.PostojiAsync(zaposlenik.Id));
        Assert.False(await repo.PostojiAsync(99999));
    }
}
