namespace ChainTrack.BLL.Exceptions;

/// <summary>
/// Iznimka koju poslovni sloj baca kad bi tražena operacija prekršila
/// poslovno pravilo (npr. brisanje aktivnog zaposlenika ili lokacije
/// s aktivnim zaposlenicima).
/// </summary>
public class PoslovnoPraviloException : Exception
{
    public PoslovnoPraviloException(string poruka) : base(poruka)
    {
    }
}
