using ChainTrack.BLL.Exceptions;

namespace ChainTrack.BLL.Validation;

/// <summary>
/// Akumulator pogrešaka validacije. Poslovni sloj njime skuplja sve pronađene
/// pogreške po pojedinom polju i, na kraju, baca <see cref="ValidacijaException"/>
/// ako ijedna pogreška postoji.
/// </summary>
public class ValidacijaRezultat
{
    private readonly Dictionary<string, List<string>> _greske = new();

    public bool Valjano => _greske.Count == 0;

    public IReadOnlyDictionary<string, List<string>> Greske => _greske;

    /// <summary>Bilježi pogrešku za zadano polje.</summary>
    public ValidacijaRezultat Dodaj(string polje, string poruka)
    {
        if (!_greske.TryGetValue(polje, out var lista))
        {
            lista = new List<string>();
            _greske[polje] = lista;
        }
        lista.Add(poruka);
        return this;
    }

    /// <summary>Baca <see cref="ValidacijaException"/> ako je zabilježena bilo koja pogreška.</summary>
    public void BaciAkoNijeValjano()
    {
        if (!Valjano)
            throw new ValidacijaException(_greske);
    }
}
