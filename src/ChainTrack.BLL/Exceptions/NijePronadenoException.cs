namespace ChainTrack.BLL.Exceptions;

/// <summary>Iznimka koju poslovni sloj baca kad traženi zapis ne postoji.</summary>
public class NijePronadenoException : Exception
{
    public NijePronadenoException(string poruka) : base(poruka)
    {
    }
}
