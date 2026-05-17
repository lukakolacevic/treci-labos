namespace ChainTrack.Domain.Entities;

/// <summary>Dnevni unos prihoda po lokaciji i kategoriji.</summary>
public class Prihod
{
    public int Id { get; set; }

    public int LokacijaId { get; set; }
    public Lokacija? Lokacija { get; set; }

    public int KategorijaId { get; set; }
    public KategorijaPrihoda? Kategorija { get; set; }

    public DateOnly Datum { get; set; }
    public decimal Iznos { get; set; }
    public string? Napomena { get; set; }

    public int? UnioKorisnikId { get; set; }
    public Korisnik? UnioKorisnik { get; set; }
}
