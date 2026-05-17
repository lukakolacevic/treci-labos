using ChainTrack.Domain.Enums;

namespace ChainTrack.Domain.Entities;

/// <summary>
/// Korisnik sustava. Generalizacija - specijalizacije (Administrator, VoditeljLanca,
/// VoditeljLokacije) razlikuju se atributom <see cref="Uloga"/>.
/// </summary>
public class Korisnik
{
    public int Id { get; set; }
    public string KorisnickoIme { get; set; } = string.Empty;
    public string LozinkaHash { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UlogaKorisnika Uloga { get; set; }
    public bool Aktivan { get; set; } = true;

    // Navigacijska svojstva
    public ICollection<Lokacija> VodjeneLokacije { get; set; } = new List<Lokacija>();
    public ICollection<Prihod> UneseniPrihodi { get; set; } = new List<Prihod>();

    public string PunoIme => $"{Ime} {Prezime}";
}
