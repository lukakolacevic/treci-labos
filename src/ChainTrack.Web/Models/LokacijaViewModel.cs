using System.ComponentModel.DataAnnotations;
using ChainTrack.Domain.Enums;

namespace ChainTrack.Web.Models;

/// <summary>
/// Model prikaza zaglavlja složenog (master-detail) ekrana - lokacija.
/// Atributi DataAnnotations daju osnovnu (prezentacijsku) validaciju;
/// složena poslovna pravila provjerava poslovni sloj.
/// </summary>
public class LokacijaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(100)]
    [Display(Name = "Naziv")]
    public string Naziv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa je obavezna.")]
    [StringLength(150)]
    [Display(Name = "Adresa")]
    public string Adresa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(50)]
    [Display(Name = "Grad")]
    public string Grad { get; set; } = string.Empty;

    [Display(Name = "Tip objekta")]
    public TipObjekta TipObjekta { get; set; } = TipObjekta.RESTORAN;

    [Display(Name = "Aktivna")]
    public bool Aktivna { get; set; } = true;

    [Display(Name = "Voditelj lokacije")]
    public int? VoditeljId { get; set; }
}
