using ChainTrack.Domain.Enums;

namespace ChainTrack.Domain.Entities;

/// <summary>
/// Zaposlenik vezan uz točno jednu lokaciju i jedan tip. Entitet ekrana šifrarnika
/// (vidi DZ3 specifikaciju). OIB se validira algoritmom kontrolne znamenke.
/// </summary>
public class Zaposlenik
{
    public int Id { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;

    /// <summary>Osobni identifikacijski broj - 11 znamenki, provjera kontrolne znamenke (ISO 7064, MOD 11,10).</summary>
    public string Oib { get; set; } = string.Empty;

    public DateOnly DatumZaposlenja { get; set; }
    public decimal Placa { get; set; }

    /// <summary>Strani ključ na tip zaposlenika (odabir putem padajuće liste).</summary>
    public int TipId { get; set; }
    public TipZaposlenika? Tip { get; set; }

    /// <summary>Strani ključ na lokaciju (odabir putem padajuće liste).</summary>
    public int LokacijaId { get; set; }
    public Lokacija? Lokacija { get; set; }

    public StatusZaposlenika Status { get; set; } = StatusZaposlenika.AKTIVAN;

    public string PunoIme => $"{Ime} {Prezime}";
}
