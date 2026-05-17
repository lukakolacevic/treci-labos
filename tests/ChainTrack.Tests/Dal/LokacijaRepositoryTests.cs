using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using ChainTrack.Tests.TestSupport;

namespace ChainTrack.Tests.Dal;

/// <summary>
/// JEDINIČNI TESTOVI SLOJA ZA PRISTUP PODACIMA (DAL).
/// Repozitorij <see cref="LokacijaRepository"/> testira se nad stvarnom
/// (SQLite, u memoriji) bazom podataka.
/// </summary>
public class LokacijaRepositoryTests
{
    [Fact]
    public async Task DodajAsync_trajnoSpremaLokaciju()
    {
        using var db = new SqliteTestContext();

        var repo = new LokacijaRepository(db.NoviKontekst());
        var nova = new Lokacija
        {
            Naziv = "Nova lokacija", Adresa = "Adresa 9", Grad = "Split",
            TipObjekta = TipObjekta.CAFFE_BAR
        };
        await repo.DodajAsync(nova);

        // Provjera neovisnim kontekstom - dokaz da je zapis stvarno u bazi.
        var provjera = new LokacijaRepository(db.NoviKontekst());
        var dohvacena = await provjera.DohvatiAsync(nova.Id);

        Assert.NotNull(dohvacena);
        Assert.Equal("Nova lokacija", dohvacena!.Naziv);
    }

    [Fact]
    public async Task PretraziAsync_filtriraPoPojmu()
    {
        using var db = new SqliteTestContext();
        using (var ctx = db.NoviKontekst())
        {
            ctx.Lokacije.AddRange(
                new Lokacija { Naziv = "Burger Zagreb", Adresa = "x", Grad = "Zagreb", TipObjekta = TipObjekta.FAST_FOOD },
                new Lokacija { Naziv = "Pizza Split", Adresa = "x", Grad = "Split", TipObjekta = TipObjekta.RESTORAN },
                new Lokacija { Naziv = "Caffe Zagreb", Adresa = "x", Grad = "Zagreb", TipObjekta = TipObjekta.CAFFE_BAR });
            ctx.SaveChanges();
        }
        var repo = new LokacijaRepository(db.NoviKontekst());

        var rezultat = await repo.PretraziAsync("zagreb", 1, 20);

        Assert.Equal(2, rezultat.UkupnoZapisa);
        Assert.All(rezultat.Stavke, l => Assert.Equal("Zagreb", l.Grad));
    }

    [Fact]
    public async Task PretraziAsync_vracaTrazenuStranicu()
    {
        using var db = new SqliteTestContext();
        using (var ctx = db.NoviKontekst())
        {
            for (int i = 1; i <= 25; i++)
                ctx.Lokacije.Add(new Lokacija
                {
                    Naziv = $"Lokacija {i:D2}", Adresa = "x", Grad = "Grad",
                    TipObjekta = TipObjekta.RESTORAN
                });
            ctx.SaveChanges();
        }
        var repo = new LokacijaRepository(db.NoviKontekst());

        var drugaStranica = await repo.PretraziAsync(null, stranica: 2, velicinaStranice: 20);

        Assert.Equal(25, drugaStranica.UkupnoZapisa);
        Assert.Equal(2, drugaStranica.Stranica);
        Assert.Equal(5, drugaStranica.Stavke.Count);
        Assert.Equal(2, drugaStranica.UkupnoStranica);
    }

    [Fact]
    public async Task DohvatiSDetaljimaAsync_ukljucujeZaliheISastojke()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        using (var ctx = db.NoviKontekst())
        {
            ctx.ZaliheSastojaka.Add(new ZalihaSastojka
            {
                LokacijaId = okruzenje.Lokacija.Id, SastojakId = okruzenje.SastojakA.Id,
                Kolicina = 10m, MinimalnaRazina = 2m, Azurirano = DateTime.UtcNow
            });
            ctx.SaveChanges();
        }
        var repo = new LokacijaRepository(db.NoviKontekst());

        var lokacija = await repo.DohvatiSDetaljimaAsync(okruzenje.Lokacija.Id);

        Assert.NotNull(lokacija);
        Assert.Single(lokacija!.Zalihe);
        Assert.Equal("Sastojak A", lokacija.Zalihe.First().Sastojak!.Naziv);
    }

    [Fact]
    public async Task ObrisiAsync_uklanjaLokaciju()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        var repo = new LokacijaRepository(db.NoviKontekst());

        await repo.ObrisiAsync(okruzenje.Lokacija.Id);

        var provjera = new LokacijaRepository(db.NoviKontekst());
        Assert.Null(await provjera.DohvatiAsync(okruzenje.Lokacija.Id));
    }

    [Fact]
    public async Task BrojAktivnihZaposlenikaAsync_brojiSamoAktivne()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        using (var ctx = db.NoviKontekst())
        {
            ctx.Zaposlenici.AddRange(
                new Zaposlenik { Ime = "A", Prezime = "A", Oib = "12345678903", DatumZaposlenja = new DateOnly(2024, 1, 1), Placa = 1000m, TipId = okruzenje.Tip.Id, LokacijaId = okruzenje.Lokacija.Id, Status = StatusZaposlenika.AKTIVAN },
                new Zaposlenik { Ime = "B", Prezime = "B", Oib = "26039951106", DatumZaposlenja = new DateOnly(2024, 1, 1), Placa = 1000m, TipId = okruzenje.Tip.Id, LokacijaId = okruzenje.Lokacija.Id, Status = StatusZaposlenika.NEAKTIVAN });
            ctx.SaveChanges();
        }
        var repo = new LokacijaRepository(db.NoviKontekst());

        int aktivnih = await repo.BrojAktivnihZaposlenikaAsync(okruzenje.Lokacija.Id);

        Assert.Equal(1, aktivnih);
    }
}
