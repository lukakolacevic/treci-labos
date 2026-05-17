using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Entities;
using ChainTrack.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.Tests.Dal;

/// <summary>JEDINIČNI TESTOVI SLOJA ZA PRISTUP PODACIMA (DAL) - zalihe sastojaka.</summary>
public class ZalihaSastojkaRepositoryTests
{
    [Fact]
    public async Task DodajAsync_trajnoSpremaStavkuZalihe()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);

        var repo = new ZalihaSastojkaRepository(db.NoviKontekst());
        var zaliha = new ZalihaSastojka
        {
            LokacijaId = okruzenje.Lokacija.Id, SastojakId = okruzenje.SastojakA.Id,
            Kolicina = 7.5m, MinimalnaRazina = 2m, Azurirano = DateTime.UtcNow
        };
        await repo.DodajAsync(zaliha);

        var provjera = new ZalihaSastojkaRepository(db.NoviKontekst());
        Assert.NotNull(await provjera.DohvatiAsync(zaliha.Id));
    }

    [Fact]
    public async Task PostojiZaSastojakAsync_detektiraPostojeciPar()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        using (var ctx = db.NoviKontekst())
        {
            ctx.ZaliheSastojaka.Add(new ZalihaSastojka
            {
                LokacijaId = okruzenje.Lokacija.Id, SastojakId = okruzenje.SastojakA.Id,
                Kolicina = 5m, MinimalnaRazina = 1m, Azurirano = DateTime.UtcNow
            });
            ctx.SaveChanges();
        }
        var repo = new ZalihaSastojkaRepository(db.NoviKontekst());

        Assert.True(await repo.PostojiZaSastojakAsync(okruzenje.Lokacija.Id, okruzenje.SastojakA.Id));
        Assert.False(await repo.PostojiZaSastojakAsync(okruzenje.Lokacija.Id, okruzenje.SastojakB.Id));
    }

    [Fact]
    public async Task PostojiZaSastojakAsync_iskljucujeZadanuStavku()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        ZalihaSastojka stavka;
        using (var ctx = db.NoviKontekst())
        {
            stavka = new ZalihaSastojka
            {
                LokacijaId = okruzenje.Lokacija.Id, SastojakId = okruzenje.SastojakA.Id,
                Kolicina = 5m, MinimalnaRazina = 1m, Azurirano = DateTime.UtcNow
            };
            ctx.ZaliheSastojaka.Add(stavka);
            ctx.SaveChanges();
        }
        var repo = new ZalihaSastojkaRepository(db.NoviKontekst());

        // Pri uređivanju iste stavke njezin par ne smije se tretirati kao duplikat.
        Assert.False(await repo.PostojiZaSastojakAsync(
            okruzenje.Lokacija.Id, okruzenje.SastojakA.Id, osimZalihaId: stavka.Id));
    }

    [Fact]
    public async Task Baza_odbijaDuplikatParaLokacijaSastojak()
    {
        using var db = new SqliteTestContext();
        var okruzenje = TestPodaci.Posijej(db);
        using var ctx = db.NoviKontekst();

        ctx.ZaliheSastojaka.Add(new ZalihaSastojka
        {
            LokacijaId = okruzenje.Lokacija.Id, SastojakId = okruzenje.SastojakA.Id,
            Kolicina = 1m, MinimalnaRazina = 1m, Azurirano = DateTime.UtcNow
        });
        ctx.ZaliheSastojaka.Add(new ZalihaSastojka
        {
            LokacijaId = okruzenje.Lokacija.Id, SastojakId = okruzenje.SastojakA.Id,
            Kolicina = 2m, MinimalnaRazina = 1m, Azurirano = DateTime.UtcNow
        });

        // Jedinstveni indeks (lokacija_id, sastojak_id) brani upis duplikata.
        await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}
