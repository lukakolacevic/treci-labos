namespace ChainTrack.BLL.Validation;

/// <summary>
/// Provjera ispravnosti hrvatskog OIB-a. Ovo je primjer <b>složene validacije</b>
/// tražene u DZ3 - ne provjerava se samo je li polje popunjeno ili u rasponu,
/// nego se izračunava i uspoređuje kontrolna (zadnja) znamenka.
/// <para>
/// Algoritam: ISO 7064, MOD 11,10 (međunarodna norma za kontrolne znamenke;
/// u zahtjevima DZ1, FR-08, neformalno nazvan "Luhn za HR OIB").
/// </para>
/// </summary>
public static class OibValidator
{
    /// <summary>Vraća <c>true</c> ako je OIB ispravan (11 znamenki + točna kontrolna znamenka).</summary>
    public static bool JeValjan(string? oib)
    {
        if (string.IsNullOrWhiteSpace(oib))
            return false;

        oib = oib.Trim();

        if (oib.Length != 11 || !oib.All(char.IsDigit))
            return false;

        // Izračun kontrolne znamenke prvih 10 znamenki (ISO 7064, MOD 11,10).
        int medurezultat = 10;
        for (int i = 0; i < 10; i++)
        {
            medurezultat += oib[i] - '0';
            medurezultat %= 10;
            if (medurezultat == 0)
                medurezultat = 10;
            medurezultat = (medurezultat * 2) % 11;
        }

        int kontrolnaZnamenka = (11 - medurezultat) % 10;
        int unesenaZnamenka = oib[10] - '0';

        return kontrolnaZnamenka == unesenaZnamenka;
    }
}
