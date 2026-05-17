using System.ComponentModel.DataAnnotations;
using ChainTrack.Domain.Enums;

namespace ChainTrack.Web.Models;

/// <summary>
/// Model prikaza ekrana šifrarnika zaposlenika. Osnovnu validaciju daju
/// DataAnnotations, a složenu (ispravnost OIB-a, jedinstvenost) poslovni sloj.
/// </summary>
public class ZaposlenikViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ime je obavezno.")]
    [StringLength(50)]
    [Display(Name = "Ime")]
    public string Ime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno.")]
    [StringLength(50)]
    [Display(Name = "Prezime")]
    public string Prezime { get; set; } = string.Empty;

    [Required(ErrorMessage = "OIB je obavezan.")]
    [RegularExpression(@"\d{11}", ErrorMessage = "OIB mora imati točno 11 znamenki.")]
    [Display(Name = "OIB")]
    public string Oib { get; set; } = string.Empty;

    [Display(Name = "Datum zaposlenja")]
    [DataType(DataType.Date)]
    public DateOnly DatumZaposlenja { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Plaća (EUR)")]
    [Range(0.01, 1_000_000, ErrorMessage = "Plaća mora biti veća od nule.")]
    public decimal Placa { get; set; }

    [Display(Name = "Tip zaposlenika")]
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite tip zaposlenika.")]
    public int TipId { get; set; }

    [Display(Name = "Lokacija")]
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite lokaciju.")]
    public int LokacijaId { get; set; }

    [Display(Name = "Status")]
    public StatusZaposlenika Status { get; set; } = StatusZaposlenika.AKTIVAN;
}
