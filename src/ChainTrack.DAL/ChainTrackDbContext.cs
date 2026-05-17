using ChainTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.DAL;

/// <summary>
/// EF Core kontekst baze podataka - jezgra sloja za pristup podacima (DAL).
/// Mapira domenske entitete na relacijsku shemu opisanu u DZ2 (database/create.sql).
/// Kontekst je neovisan o pružatelju baze (PostgreSQL u pogonu, SQLite u testovima) -
/// pružatelj se odabire u kompozicijskom korijenu (Web odn. test projektu).
/// </summary>
public class ChainTrackDbContext : DbContext
{
    public ChainTrackDbContext(DbContextOptions<ChainTrackDbContext> options) : base(options) { }

    public DbSet<Korisnik> Korisnici => Set<Korisnik>();
    public DbSet<Lokacija> Lokacije => Set<Lokacija>();
    public DbSet<TipZaposlenika> TipoviZaposlenika => Set<TipZaposlenika>();
    public DbSet<Zaposlenik> Zaposlenici => Set<Zaposlenik>();
    public DbSet<KategorijaSastojka> KategorijeSastojaka => Set<KategorijaSastojka>();
    public DbSet<Sastojak> Sastojci => Set<Sastojak>();
    public DbSet<ZalihaSastojka> ZaliheSastojaka => Set<ZalihaSastojka>();
    public DbSet<KategorijaPrihoda> KategorijePrihoda => Set<KategorijaPrihoda>();
    public DbSet<Prihod> Prihodi => Set<Prihod>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Korisnik>(e =>
        {
            e.ToTable("korisnik");
            e.HasKey(k => k.Id);
            e.Property(k => k.Id).HasColumnName("id");
            e.Property(k => k.KorisnickoIme).HasColumnName("korisnicko_ime").HasMaxLength(50).IsRequired();
            e.Property(k => k.LozinkaHash).HasColumnName("lozinka_hash").HasMaxLength(255).IsRequired();
            e.Property(k => k.Ime).HasColumnName("ime").HasMaxLength(50).IsRequired();
            e.Property(k => k.Prezime).HasColumnName("prezime").HasMaxLength(50).IsRequired();
            e.Property(k => k.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            e.Property(k => k.Uloga).HasColumnName("uloga").HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(k => k.Aktivan).HasColumnName("aktivan").IsRequired();
            e.Ignore(k => k.PunoIme);
            e.HasIndex(k => k.KorisnickoIme).IsUnique();
            e.HasIndex(k => k.Email).IsUnique();
        });

        b.Entity<Lokacija>(e =>
        {
            e.ToTable("lokacija");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasColumnName("id");
            e.Property(l => l.Naziv).HasColumnName("naziv").HasMaxLength(100).IsRequired();
            e.Property(l => l.Adresa).HasColumnName("adresa").HasMaxLength(150).IsRequired();
            e.Property(l => l.Grad).HasColumnName("grad").HasMaxLength(50).IsRequired();
            e.Property(l => l.TipObjekta).HasColumnName("tip_objekta").HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(l => l.Aktivna).HasColumnName("aktivna").IsRequired();
            e.Property(l => l.VoditeljId).HasColumnName("voditelj_id");
            e.HasOne(l => l.Voditelj)
                .WithMany(k => k.VodjeneLokacije)
                .HasForeignKey(l => l.VoditeljId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<TipZaposlenika>(e =>
        {
            e.ToTable("tip_zaposlenika");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.Naziv).HasColumnName("naziv").HasMaxLength(50).IsRequired();
            e.Property(t => t.Opis).HasColumnName("opis").HasMaxLength(255);
            e.Property(t => t.RazinaPristupa).HasColumnName("razina_pristupa").IsRequired();
            e.HasIndex(t => t.Naziv).IsUnique();
        });

        b.Entity<Zaposlenik>(e =>
        {
            e.ToTable("zaposlenik", t =>
                t.HasCheckConstraint("ck_zaposlenik_placa", "placa >= 0"));
            e.HasKey(z => z.Id);
            e.Property(z => z.Id).HasColumnName("id");
            e.Property(z => z.Ime).HasColumnName("ime").HasMaxLength(50).IsRequired();
            e.Property(z => z.Prezime).HasColumnName("prezime").HasMaxLength(50).IsRequired();
            e.Property(z => z.Oib).HasColumnName("oib").HasMaxLength(11).IsFixedLength().IsRequired();
            e.Property(z => z.DatumZaposlenja).HasColumnName("datum_zaposlenja").IsRequired();
            e.Property(z => z.Placa).HasColumnName("placa").HasPrecision(10, 2).IsRequired();
            e.Property(z => z.TipId).HasColumnName("tip_id").IsRequired();
            e.Property(z => z.LokacijaId).HasColumnName("lokacija_id").IsRequired();
            e.Property(z => z.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(15).IsRequired();
            e.Ignore(z => z.PunoIme);
            e.HasIndex(z => z.Oib).IsUnique();
            e.HasOne(z => z.Tip)
                .WithMany(t => t.Zaposlenici)
                .HasForeignKey(z => z.TipId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(z => z.Lokacija)
                .WithMany(l => l.Zaposlenici)
                .HasForeignKey(z => z.LokacijaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<KategorijaSastojka>(e =>
        {
            e.ToTable("kategorija_sastojka");
            e.HasKey(k => k.Id);
            e.Property(k => k.Id).HasColumnName("id");
            e.Property(k => k.Naziv).HasColumnName("naziv").HasMaxLength(50).IsRequired();
            e.Property(k => k.Opis).HasColumnName("opis").HasMaxLength(255);
            e.HasIndex(k => k.Naziv).IsUnique();
        });

        b.Entity<Sastojak>(e =>
        {
            e.ToTable("sastojak");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.Naziv).HasColumnName("naziv").HasMaxLength(100).IsRequired();
            e.Property(s => s.KategorijaId).HasColumnName("kategorija_id").IsRequired();
            e.Property(s => s.JedinicaMjere).HasColumnName("jedinica_mjere").HasMaxLength(10).IsRequired();
            e.HasOne(s => s.Kategorija)
                .WithMany(k => k.Sastojci)
                .HasForeignKey(s => s.KategorijaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ZalihaSastojka>(e =>
        {
            e.ToTable("zaliha_sastojka", t =>
            {
                t.HasCheckConstraint("ck_zaliha_kolicina", "kolicina >= 0");
                t.HasCheckConstraint("ck_zaliha_min_razina", "minimalna_razina >= 0");
            });
            e.HasKey(z => z.Id);
            e.Property(z => z.Id).HasColumnName("id");
            e.Property(z => z.LokacijaId).HasColumnName("lokacija_id").IsRequired();
            e.Property(z => z.SastojakId).HasColumnName("sastojak_id").IsRequired();
            e.Property(z => z.Kolicina).HasColumnName("kolicina").HasPrecision(10, 2).IsRequired();
            e.Property(z => z.MinimalnaRazina).HasColumnName("minimalna_razina").HasPrecision(10, 2).IsRequired();
            e.Property(z => z.Azurirano).HasColumnName("azurirano").IsRequired();
            // Spojni entitet: par (lokacija, sastojak) mora biti jedinstven.
            e.HasIndex(z => new { z.LokacijaId, z.SastojakId }).IsUnique();
            e.HasOne(z => z.Lokacija)
                .WithMany(l => l.Zalihe)
                .HasForeignKey(z => z.LokacijaId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(z => z.Sastojak)
                .WithMany(s => s.Zalihe)
                .HasForeignKey(z => z.SastojakId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<KategorijaPrihoda>(e =>
        {
            e.ToTable("kategorija_prihoda");
            e.HasKey(k => k.Id);
            e.Property(k => k.Id).HasColumnName("id");
            e.Property(k => k.Naziv).HasColumnName("naziv").HasMaxLength(50).IsRequired();
            e.HasIndex(k => k.Naziv).IsUnique();
        });

        b.Entity<Prihod>(e =>
        {
            e.ToTable("prihod", t =>
                t.HasCheckConstraint("ck_prihod_iznos", "iznos >= 0"));
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.LokacijaId).HasColumnName("lokacija_id").IsRequired();
            e.Property(p => p.KategorijaId).HasColumnName("kategorija_id").IsRequired();
            e.Property(p => p.Datum).HasColumnName("datum").IsRequired();
            e.Property(p => p.Iznos).HasColumnName("iznos").HasPrecision(12, 2).IsRequired();
            e.Property(p => p.Napomena).HasColumnName("napomena").HasMaxLength(255);
            e.Property(p => p.UnioKorisnikId).HasColumnName("unio_korisnik_id");
            e.HasOne(p => p.Lokacija)
                .WithMany(l => l.Prihodi)
                .HasForeignKey(p => p.LokacijaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Kategorija)
                .WithMany(k => k.Prihodi)
                .HasForeignKey(p => p.KategorijaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.UnioKorisnik)
                .WithMany(k => k.UneseniPrihodi)
                .HasForeignKey(p => p.UnioKorisnikId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
