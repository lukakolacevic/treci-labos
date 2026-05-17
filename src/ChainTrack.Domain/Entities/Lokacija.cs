using ChainTrack.Domain.Enums;

namespace ChainTrack.Domain.Entities;

/// <summary>
/// Pojedini ugostiteljski objekt lanca (restoran / fast food / caffe bar).
/// Zaglavlje složenog (master-detail) ekrana - vidi DZ3 specifikaciju.
/// </summary>
public class Lokacija
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Adresa { get; set; } = string.Empty;
    public string Grad { get; set; } = string.Empty;
    public TipObjekta TipObjekta { get; set; }
    public bool Aktivna { get; set; } = true;

    /// <summary>Strani ključ na korisnika - voditelja lokacije (odabir putem padajuće liste).</summary>
    public int? VoditeljId { get; set; }
    public Korisnik? Voditelj { get; set; }

    // Navigacijska svojstva
    public ICollection<Zaposlenik> Zaposlenici { get; set; } = new List<Zaposlenik>();
    public ICollection<ZalihaSastojka> Zalihe { get; set; } = new List<ZalihaSastojka>();
    public ICollection<Prihod> Prihodi { get; set; } = new List<Prihod>();
}
