using System.ComponentModel.DataAnnotations;

namespace ChainTrack.Web.Models;

/// <summary>
/// Model prikaza retka detalja složenog ekrana - stavka zalihe sastojka
/// na pojedinoj lokaciji.
/// </summary>
public class ZalihaSastojkaViewModel
{
    public int Id { get; set; }

    /// <summary>Strani ključ na zaglavlje (lokaciju) - prenosi se skrivenim poljem.</summary>
    public int LokacijaId { get; set; }

    [Display(Name = "Sastojak")]
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite sastojak.")]
    public int SastojakId { get; set; }

    [Display(Name = "Količina")]
    [Range(0, 9_999_999, ErrorMessage = "Količina ne može biti negativna.")]
    public decimal Kolicina { get; set; }

    [Display(Name = "Minimalna razina")]
    [Range(0, 9_999_999, ErrorMessage = "Minimalna razina ne može biti negativna.")]
    public decimal MinimalnaRazina { get; set; }
}
