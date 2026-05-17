namespace ChainTrack.Domain.Common;

/// <summary>
/// Straniceni rezultat pretrage. Pretraga na preglednim tablicama prikazuje
/// rezultate stranicno (NFR-02 / FR-07: 20 zapisa po stranici).
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Stavke { get; init; } = new List<T>();
    public int UkupnoZapisa { get; init; }
    public int Stranica { get; init; } = 1;
    public int VelicinaStranice { get; init; } = 20;

    public int UkupnoStranica =>
        VelicinaStranice <= 0 ? 1 : (int)Math.Ceiling(UkupnoZapisa / (double)VelicinaStranice);

    public bool ImaPrethodnu => Stranica > 1;
    public bool ImaSljedecu => Stranica < UkupnoStranica;
}
