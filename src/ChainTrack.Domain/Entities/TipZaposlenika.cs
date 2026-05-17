namespace ChainTrack.Domain.Entities;

/// <summary>Šifrarnik tipova zaposlenika (konobar, kuhar, menadžer ...).</summary>
public class TipZaposlenika
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public int RazinaPristupa { get; set; } = 1;

    public ICollection<Zaposlenik> Zaposlenici { get; set; } = new List<Zaposlenik>();
}
