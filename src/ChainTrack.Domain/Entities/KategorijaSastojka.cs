namespace ChainTrack.Domain.Entities;

/// <summary>Šifrarnik kategorija sastojaka (mesni proizvodi, povrće, piće ...).</summary>
public class KategorijaSastojka
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }

    public ICollection<Sastojak> Sastojci { get; set; } = new List<Sastojak>();
}
