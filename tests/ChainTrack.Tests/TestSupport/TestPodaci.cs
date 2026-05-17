using ChainTrack.DAL;
using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;

namespace ChainTrack.Tests.TestSupport;

/// <summary>Ogledni podaci za testove (minimalni, kontrolirani skup).</summary>
public static class TestPodaci
{
    public static OglednoOkruzenje Posijej(SqliteTestContext db)
    {
        using var ctx = db.NoviKontekst();

        var voditelj = new Korisnik
        {
            KorisnickoIme = "voditelj1", LozinkaHash = "hash", Ime = "Vana", Prezime = "Vodic",
            Email = "voditelj1@chaintrack.hr", Uloga = UlogaKorisnika.VODITELJ_LOKACIJE
        };
        var lokacija = new Lokacija
        {
            Naziv = "Test Lokacija", Adresa = "Ulica 1", Grad = "Zagreb",
            TipObjekta = TipObjekta.RESTORAN, Aktivna = true, Voditelj = voditelj
        };
        var tip = new TipZaposlenika { Naziv = "Kuhar", Opis = "Priprema jela", RazinaPristupa = 1 };
        var kategorija = new KategorijaSastojka { Naziv = "Test kategorija" };
        var sastojakA = new Sastojak { Naziv = "Sastojak A", Kategorija = kategorija, JedinicaMjere = "kg" };
        var sastojakB = new Sastojak { Naziv = "Sastojak B", Kategorija = kategorija, JedinicaMjere = "kom" };

        ctx.AddRange(voditelj, lokacija, tip, kategorija, sastojakA, sastojakB);
        ctx.SaveChanges();

        return new OglednoOkruzenje(voditelj, lokacija, tip, kategorija, sastojakA, sastojakB);
    }
}

/// <summary>Reference na ogledne zapise zajedno s dodijeljenim identifikatorima.</summary>
public record OglednoOkruzenje(
    Korisnik Voditelj,
    Lokacija Lokacija,
    TipZaposlenika Tip,
    KategorijaSastojka Kategorija,
    Sastojak SastojakA,
    Sastojak SastojakB);
