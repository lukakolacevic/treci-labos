namespace ChainTrack.Domain.Enums;

/// <summary>
/// Uloga korisnika sustava. Specijalizacija generalizacije <c>Korisnik</c>
/// (vidi ER model, DZ2 - poglavlje 1.1).
/// </summary>
public enum UlogaKorisnika
{
    ADMINISTRATOR,
    VODITELJ_LANCA,
    VODITELJ_LOKACIJE
}
