namespace ChainTrack.Domain.Enums;

/// <summary>
/// Stanje zapisa o zaposleniku. Vidi dijagram promjene stanja (DZ1 - poglavlje 6):
/// aktivnog zaposlenika nije dozvoljeno izravno obrisati - mora se prvo deaktivirati.
/// </summary>
public enum StatusZaposlenika
{
    AKTIVAN,
    NEAKTIVAN
}
