namespace ChainTrack.BLL.Exceptions;

/// <summary>
/// Iznimka koju poslovni sloj baca kad uneseni podaci ne zadovoljavaju
/// validacijska pravila. Nosi pogreške grupirane po nazivu polja, pa ih
/// prezentacijski sloj može preslikati u ModelState i prikazati uz polja.
/// </summary>
public class ValidacijaException : Exception
{
    public IReadOnlyDictionary<string, string[]> Greske { get; }

    public ValidacijaException(IDictionary<string, List<string>> greske)
        : base("Uneseni podaci nisu valjani.")
    {
        Greske = greske.ToDictionary(p => p.Key, p => p.Value.ToArray());
    }

    public ValidacijaException(string polje, string poruka)
        : this(new Dictionary<string, List<string>> { [polje] = new() { poruka } })
    {
    }
}
