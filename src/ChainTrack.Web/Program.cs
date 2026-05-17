using System.Globalization;
using ChainTrack.BLL.Services;
using ChainTrack.DAL;
using ChainTrack.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

// Nepromjenjiva kultura -> decimalni separator je uvijek "." (neovisno o OS-u).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// === PREZENTACIJSKI SLOJ ===
builder.Services.AddControllersWithViews();

// === SLOJ ZA PRISTUP PODACIMA (DAL) - EF Core kontekst ===
// Pružatelj baze bira se konfiguracijom: "Postgres" (zadano, prema DZ2) ili
// "Sqlite" (za pokretanje bez zasebnog poslužitelja baze).
string provider = builder.Configuration["DatabaseProvider"] ?? "Postgres";
builder.Services.AddDbContext<ChainTrackDbContext>(options =>
{
    if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
                          ?? "Data Source=chaintrack.db");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")
                          ?? "Host=localhost;Port=5432;Database=chaintrack;Username=postgres;Password=postgres");
    }
});

// Repozitoriji sloja za pristup podacima.
builder.Services.AddScoped<ILokacijaRepository, LokacijaRepository>();
builder.Services.AddScoped<IZalihaSastojkaRepository, ZalihaSastojkaRepository>();
builder.Services.AddScoped<IZaposlenikRepository, ZaposlenikRepository>();
builder.Services.AddScoped<ISifrarnikRepository, SifrarnikRepository>();

// === POSLOVNI SLOJ (BLL) - servisi ===
builder.Services.AddScoped<ILokacijaService, LokacijaService>();
builder.Services.AddScoped<IZaposlenikService, ZaposlenikService>();
builder.Services.AddScoped<ISifrarnikService, SifrarnikService>();

var app = builder.Build();

// Priprema baze: kreiranje sheme i inicijalno punjenje oglednim podacima.
await PripremiBazuAsync(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// --- Lokalna funkcija: priprema baze podataka ---
static async Task PripremiBazuAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ChainTrackDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var creator = db.Database.GetService<IRelationalDatabaseCreator>();

    // Pričekaj da baza postane dostupna (npr. dok se Docker kontejner ne podigne).
    for (int pokusaj = 1; ; pokusaj++)
    {
        try
        {
            if (!await creator.ExistsAsync())
                await creator.CreateAsync();
            break;
        }
        catch (Exception ex) when (pokusaj < 20)
        {
            logger.LogWarning("Baza još nije dostupna (pokušaj {Pokusaj}): {Poruka}", pokusaj, ex.Message);
            await Task.Delay(2000);
        }
    }

    // Kreiraj tablice ako shema još ne postoji.
    if (!await creator.HasTablesAsync())
        await creator.CreateTablesAsync();

    await DatabaseSeeder.SeedAsync(db);
    logger.LogInformation("Baza podataka je spremna.");
}

// Učinjeno javnim radi integracijskih testova (WebApplicationFactory).
public partial class Program { }
