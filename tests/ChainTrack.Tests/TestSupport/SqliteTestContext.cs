using ChainTrack.DAL;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChainTrack.Tests.TestSupport;

/// <summary>
/// Za svaki test stvara svježu SQLite bazu podataka u memoriji. Time su testovi
/// međusobno neovisni i mogu se pokretati više puta (uvjet iz DZ3).
/// Baza živi dok je veza otvorena; <see cref="NoviKontekst"/> daje nove instance
/// konteksta nad istom bazom (za provjeru da su podaci stvarno trajno spremljeni).
/// </summary>
public sealed class SqliteTestContext : IDisposable
{
    private readonly SqliteConnection _veza;
    private readonly DbContextOptions<ChainTrackDbContext> _opcije;

    public SqliteTestContext()
    {
        _veza = new SqliteConnection("DataSource=:memory:");
        _veza.Open();

        // Uključi provjeru stranih ključeva za otvorenu vezu.
        using (var naredba = _veza.CreateCommand())
        {
            naredba.CommandText = "PRAGMA foreign_keys = ON;";
            naredba.ExecuteNonQuery();
        }

        _opcije = new DbContextOptionsBuilder<ChainTrackDbContext>()
            .UseSqlite(_veza)
            .Options;

        using var kontekst = new ChainTrackDbContext(_opcije);
        kontekst.Database.EnsureCreated();
    }

    /// <summary>Nova instanca konteksta nad istom (zajedničkom) bazom.</summary>
    public ChainTrackDbContext NoviKontekst() => new(_opcije);

    public void Dispose() => _veza.Dispose();
}
