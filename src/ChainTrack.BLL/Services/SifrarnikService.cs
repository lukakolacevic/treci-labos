using ChainTrack.DAL.Repositories;
using ChainTrack.Domain.Entities;

namespace ChainTrack.BLL.Services;

/// <summary>
/// Poslovni sloj za dohvat šifrarničkih (lookup) podataka kojima se pune
/// padajuće liste za odabir stranog ključa na ekranima.
/// </summary>
public interface ISifrarnikService
{
    Task<IReadOnlyList<TipZaposlenika>> TipoviZaposlenikaAsync();
    Task<IReadOnlyList<Lokacija>> LokacijeAsync();
    Task<IReadOnlyList<Sastojak>> SastojciAsync();
    Task<IReadOnlyList<Korisnik>> VoditeljiAsync();
}

/// <inheritdoc cref="ISifrarnikService"/>
public class SifrarnikService : ISifrarnikService
{
    private readonly ISifrarnikRepository _repo;

    public SifrarnikService(ISifrarnikRepository repo) => _repo = repo;

    public Task<IReadOnlyList<TipZaposlenika>> TipoviZaposlenikaAsync() => _repo.TipoviZaposlenikaAsync();

    public Task<IReadOnlyList<Lokacija>> LokacijeAsync() => _repo.LokacijeAsync();

    public Task<IReadOnlyList<Sastojak>> SastojciAsync() => _repo.SastojciAsync();

    public Task<IReadOnlyList<Korisnik>> VoditeljiAsync() => _repo.VoditeljiAsync();
}
