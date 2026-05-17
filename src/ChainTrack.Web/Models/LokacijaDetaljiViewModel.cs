using ChainTrack.Domain.Entities;

namespace ChainTrack.Web.Models;

/// <summary>
/// Model prikaza cijelog složenog (master-detail) ekrana: zaglavlje (lokacija)
/// zajedno s pripadnim detaljima (stavkama zalihe) i obrascem za novu stavku.
/// </summary>
public class LokacijaDetaljiViewModel
{
    /// <summary>Zaglavlje - podaci o lokaciji (obrazac za uređivanje).</summary>
    public LokacijaViewModel Zaglavlje { get; set; } = new();

    /// <summary>Detalji - postojeće stavke zalihe na lokaciji.</summary>
    public IReadOnlyList<ZalihaSastojka> Zalihe { get; set; } = new List<ZalihaSastojka>();

    /// <summary>Obrazac za unos nove stavke zalihe.</summary>
    public ZalihaSastojkaViewModel NovaZaliha { get; set; } = new();
}
