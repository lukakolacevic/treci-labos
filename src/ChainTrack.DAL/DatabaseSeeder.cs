using ChainTrack.Domain.Entities;
using ChainTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.DAL;

/// <summary>
/// Inicijalno punjenje baze oglednim podacima (programski ekvivalent skripte
/// database/seed.sql). Izvodi se samo ako je baza prazna, pa je sigurno za
/// višekratno pokretanje aplikacije.
/// </summary>
public static class DatabaseSeeder
{
    // Ogledni hash lozinke (autentikacija nije u opsegu DZ3 - ovo je samo nenull vrijednost).
    private const string PlaceholderHash = "$2b$12$ChainTrackSeedPlaceholderHashValue.";

    public static async Task SeedAsync(ChainTrackDbContext db)
    {
        if (await db.Korisnici.AnyAsync())
            return;

        // --- Korisnici ---
        var admin = new Korisnik { KorisnickoIme = "admin", LozinkaHash = PlaceholderHash, Ime = "Ana", Prezime = "Admić", Email = "admin@chaintrack.hr", Uloga = UlogaKorisnika.ADMINISTRATOR };
        var lanac = new Korisnik { KorisnickoIme = "lanac", LozinkaHash = PlaceholderHash, Ime = "Luka", Prezime = "Lancic", Email = "luka@chaintrack.hr", Uloga = UlogaKorisnika.VODITELJ_LANCA };
        var vesna = new Korisnik { KorisnickoIme = "vlokacije", LozinkaHash = PlaceholderHash, Ime = "Vesna", Prezime = "Vodic", Email = "vesna@chaintrack.hr", Uloga = UlogaKorisnika.VODITELJ_LOKACIJE };
        var marko = new Korisnik { KorisnickoIme = "vlokacije2", LozinkaHash = PlaceholderHash, Ime = "Marko", Prezime = "Markic", Email = "marko@chaintrack.hr", Uloga = UlogaKorisnika.VODITELJ_LOKACIJE };
        db.Korisnici.AddRange(admin, lanac, vesna, marko);

        // --- Lokacije ---
        var burger = new Lokacija { Naziv = "Burger Bar Centar", Adresa = "Ilica 5", Grad = "Zagreb", TipObjekta = TipObjekta.FAST_FOOD, Aktivna = true, Voditelj = vesna };
        var pizza = new Lokacija { Naziv = "Pizza Mare", Adresa = "Riva 12", Grad = "Split", TipObjekta = TipObjekta.RESTORAN, Aktivna = true, Voditelj = marko };
        var caffe = new Lokacija { Naziv = "Caffe Sunce", Adresa = "Trg bana 1", Grad = "Zagreb", TipObjekta = TipObjekta.CAFFE_BAR, Aktivna = true, Voditelj = vesna };
        db.Lokacije.AddRange(burger, pizza, caffe);

        // --- Tipovi zaposlenika ---
        var konobar = new TipZaposlenika { Naziv = "Konobar", Opis = "Posluživanje gostiju", RazinaPristupa = 1 };
        var kuhar = new TipZaposlenika { Naziv = "Kuhar", Opis = "Priprema jela", RazinaPristupa = 1 };
        var menadzer = new TipZaposlenika { Naziv = "Menadžer", Opis = "Voditelj smjene", RazinaPristupa = 2 };
        var dostavljac = new TipZaposlenika { Naziv = "Dostavljač", Opis = "Dostava narudžbi", RazinaPristupa = 1 };
        var barmen = new TipZaposlenika { Naziv = "Barmen", Opis = "Priprema napitaka", RazinaPristupa = 1 };
        db.TipoviZaposlenika.AddRange(konobar, kuhar, menadzer, dostavljac, barmen);

        // --- Zaposlenici (OIB-ovi su valjani - prolaze provjeru kontrolne znamenke) ---
        db.Zaposlenici.AddRange(
            new Zaposlenik { Ime = "Ivan", Prezime = "Horvat", Oib = "12345678903", DatumZaposlenja = new DateOnly(2024, 3, 1), Placa = 1100.00m, Tip = konobar, Lokacija = burger, Status = StatusZaposlenika.AKTIVAN },
            new Zaposlenik { Ime = "Petra", Prezime = "Kovač", Oib = "26039951106", DatumZaposlenja = new DateOnly(2023, 9, 15), Placa = 1300.00m, Tip = kuhar, Lokacija = burger, Status = StatusZaposlenika.AKTIVAN },
            new Zaposlenik { Ime = "Mate", Prezime = "Jurkić", Oib = "65120987306", DatumZaposlenja = new DateOnly(2025, 1, 10), Placa = 1500.00m, Tip = menadzer, Lokacija = pizza, Status = StatusZaposlenika.AKTIVAN },
            new Zaposlenik { Ime = "Lara", Prezime = "Babić", Oib = "99887766550", DatumZaposlenja = new DateOnly(2024, 11, 20), Placa = 1050.00m, Tip = dostavljac, Lokacija = pizza, Status = StatusZaposlenika.AKTIVAN },
            new Zaposlenik { Ime = "Tea", Prezime = "Šorić", Oib = "10293847565", DatumZaposlenja = new DateOnly(2025, 2, 5), Placa = 1100.00m, Tip = barmen, Lokacija = caffe, Status = StatusZaposlenika.NEAKTIVAN }
        );

        // --- Kategorije i sastojci ---
        var mesni = new KategorijaSastojka { Naziv = "Mesni proizvodi", Opis = "Svinjetina, govedina, piletina" };
        var povrce = new KategorijaSastojka { Naziv = "Povrće", Opis = "Svježe povrće" };
        var mlijecni = new KategorijaSastojka { Naziv = "Mliječni", Opis = "Mlijeko, sir, vrhnje" };
        var pica = new KategorijaSastojka { Naziv = "Piće", Opis = "Bezalkoholna i alkoholna pića" };
        db.KategorijeSastojaka.AddRange(mesni, povrce, mlijecni, pica);

        var junetina = new Sastojak { Naziv = "Mljevena junetina", Kategorija = mesni, JedinicaMjere = "kg" };
        var pileci = new Sastojak { Naziv = "Pileći file", Kategorija = mesni, JedinicaMjere = "kg" };
        var rajcica = new Sastojak { Naziv = "Rajčica", Kategorija = povrce, JedinicaMjere = "kg" };
        var salata = new Sastojak { Naziv = "Salata", Kategorija = povrce, JedinicaMjere = "kg" };
        var mozzarella = new Sastojak { Naziv = "Mozzarella", Kategorija = mlijecni, JedinicaMjere = "kg" };
        var cola = new Sastojak { Naziv = "Coca-Cola 0.33L", Kategorija = pica, JedinicaMjere = "kom" };
        db.Sastojci.AddRange(junetina, pileci, rajcica, salata, mozzarella, cola);

        // --- Zalihe sastojaka po lokacijama (detalji složenog ekrana) ---
        var t = new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc);
        db.ZaliheSastojaka.AddRange(
            new ZalihaSastojka { Lokacija = burger, Sastojak = junetina, Kolicina = 12.50m, MinimalnaRazina = 5.00m, Azurirano = t },
            new ZalihaSastojka { Lokacija = burger, Sastojak = rajcica, Kolicina = 4.00m, MinimalnaRazina = 5.00m, Azurirano = t },   // ispod minimuma
            new ZalihaSastojka { Lokacija = burger, Sastojak = cola, Kolicina = 80.00m, MinimalnaRazina = 24.00m, Azurirano = t },
            new ZalihaSastojka { Lokacija = pizza, Sastojak = pileci, Kolicina = 8.00m, MinimalnaRazina = 4.00m, Azurirano = t },
            new ZalihaSastojka { Lokacija = pizza, Sastojak = mozzarella, Kolicina = 6.50m, MinimalnaRazina = 3.00m, Azurirano = t },
            new ZalihaSastojka { Lokacija = pizza, Sastojak = rajcica, Kolicina = 15.00m, MinimalnaRazina = 6.00m, Azurirano = t },
            new ZalihaSastojka { Lokacija = caffe, Sastojak = cola, Kolicina = 50.00m, MinimalnaRazina = 30.00m, Azurirano = t }
        );

        // --- Kategorije i unosi prihoda ---
        var hrana = new KategorijaPrihoda { Naziv = "Hrana" };
        var picePrihod = new KategorijaPrihoda { Naziv = "Piće" };
        var dostavaPrihod = new KategorijaPrihoda { Naziv = "Dostava" };
        var ostalo = new KategorijaPrihoda { Naziv = "Ostalo" };
        db.KategorijePrihoda.AddRange(hrana, picePrihod, dostavaPrihod, ostalo);

        db.Prihodi.AddRange(
            new Prihod { Lokacija = burger, Kategorija = hrana, Datum = new DateOnly(2026, 4, 10), Iznos = 850.50m, Napomena = "Petak promet", UnioKorisnik = vesna },
            new Prihod { Lokacija = burger, Kategorija = picePrihod, Datum = new DateOnly(2026, 4, 10), Iznos = 240.00m, UnioKorisnik = vesna },
            new Prihod { Lokacija = pizza, Kategorija = hrana, Datum = new DateOnly(2026, 4, 11), Iznos = 1320.00m, Napomena = "Vikend", UnioKorisnik = marko },
            new Prihod { Lokacija = pizza, Kategorija = dostavaPrihod, Datum = new DateOnly(2026, 4, 11), Iznos = 410.00m, Napomena = "Glovo + Wolt", UnioKorisnik = marko },
            new Prihod { Lokacija = caffe, Kategorija = picePrihod, Datum = new DateOnly(2026, 4, 12), Iznos = 560.00m, UnioKorisnik = vesna }
        );

        await db.SaveChangesAsync();
    }
}
