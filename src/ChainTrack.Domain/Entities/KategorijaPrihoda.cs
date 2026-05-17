namespace ChainTrack.Domain.Entities;

/// <summary>Šifrarnik kategorija prihoda (hrana, piće, dostava, ostalo).</summary>
public class KategorijaPrihoda
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;

    public ICollection<Prihod> Prihodi { get; set; } = new List<Prihod>();
}
