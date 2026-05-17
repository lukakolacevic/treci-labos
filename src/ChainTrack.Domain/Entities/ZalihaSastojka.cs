namespace ChainTrack.Domain.Entities;

/// <summary>
/// Spojni entitet (M:N Lokacija - Sastojak) s atributima. Redak detalja na složenom
/// (master-detail) ekranu. Par (LokacijaId, SastojakId) mora biti jedinstven.
/// </summary>
public class ZalihaSastojka
{
    public int Id { get; set; }

    /// <summary>Strani ključ na lokaciju (zaglavlje složenog ekrana).</summary>
    public int LokacijaId { get; set; }
    public Lokacija? Lokacija { get; set; }

    /// <summary>Strani ključ na sastojak (odabir putem padajuće liste u detaljima).</summary>
    public int SastojakId { get; set; }
    public Sastojak? Sastojak { get; set; }

    public decimal Kolicina { get; set; }
    public decimal MinimalnaRazina { get; set; }
    public DateTime Azurirano { get; set; }

    /// <summary>
    /// Poslovno pravilo iz CRC kartice (DZ2): zaliha je ispod minimuma kad je
    /// količina manja ili jednaka minimalnoj razini.
    /// </summary>
    public bool IspodMinimuma() => Kolicina <= MinimalnaRazina;
}
