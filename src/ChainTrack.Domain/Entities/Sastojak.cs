namespace ChainTrack.Domain.Entities;

/// <summary>Jedinstveni artikl (sastojak) s mjernom jedinicom.</summary>
public class Sastojak
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;

    public int KategorijaId { get; set; }
    public KategorijaSastojka? Kategorija { get; set; }

    public string JedinicaMjere { get; set; } = string.Empty;

    public ICollection<ZalihaSastojka> Zalihe { get; set; } = new List<ZalihaSastojka>();
}
