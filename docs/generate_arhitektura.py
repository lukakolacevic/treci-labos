"""
Generira dijagram komponenti za ChainTrack (slojevita arhitektura).
Pokretanje: python3 generate_arhitektura.py
Izlaz: arhitektura.png u istom direktoriju.
"""
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch
from matplotlib.lines import Line2D

fig, ax = plt.subplots(figsize=(14, 10))
ax.set_xlim(0, 14)
ax.set_ylim(0, 10)
ax.axis('off')

LAYER_COLORS = {
    "presentation": "#cfe2ff",
    "business":     "#d1e7dd",
    "dal":          "#fff3cd",
    "domain":       "#f8d7da",
    "external":     "#e2e3e5",
}

def layer(x, y, w, h, title, color):
    box = FancyBboxPatch((x, y), w, h,
                         boxstyle="round,pad=0.02,rounding_size=0.15",
                         linewidth=1.5, edgecolor="#333", facecolor=color, alpha=0.35)
    ax.add_patch(box)
    ax.text(x + 0.15, y + h - 0.25, title, fontsize=11, fontweight='bold', color="#222")

def component(cx, cy, w, h, name, sub=""):
    box = FancyBboxPatch((cx, cy), w, h,
                         boxstyle="round,pad=0.02,rounding_size=0.08",
                         linewidth=1.2, edgecolor="#222", facecolor="white")
    ax.add_patch(box)
    # «component» stereotip
    ax.text(cx + w/2, cy + h - 0.22, "«component»", fontsize=7,
            style='italic', color="#555", ha='center')
    ax.text(cx + w/2, cy + h/2 - 0.05, name, fontsize=9, fontweight='bold', ha='center')
    if sub:
        ax.text(cx + w/2, cy + 0.18, sub, fontsize=7, color="#444", ha='center')

def arrow(x1, y1, x2, y2, label="", style="-|>", color="#333", offset=(0,0)):
    a = FancyArrowPatch((x1, y1), (x2, y2),
                        arrowstyle=style, mutation_scale=14,
                        linewidth=1.1, color=color)
    ax.add_patch(a)
    if label:
        ax.text((x1+x2)/2 + offset[0], (y1+y2)/2 + offset[1],
                label, fontsize=7, color=color,
                ha='center', backgroundcolor='white')

# Naslov
ax.text(7, 9.55, "ChainTrack – Dijagram komponenti (slojevita arhitektura, N-tier)",
        fontsize=14, fontweight='bold', ha='center')
ax.text(7, 9.2, "ASP.NET Core 8 MVC · Entity Framework Core · PostgreSQL/SQLite",
        fontsize=9, style='italic', ha='center', color="#555")

# === Slojevi (od vrha prema dnu) ===
# Presentation
layer(0.3, 7.0, 13.4, 1.9, "Prezentacijski sloj  —  ChainTrack.Web (ASP.NET Core MVC)", LAYER_COLORS["presentation"])
component(0.7,  7.15, 2.3, 1.3, "HomeController")
component(3.2,  7.15, 2.6, 1.3, "LokacijeController", "Master-Detail")
component(6.0,  7.15, 2.6, 1.3, "ZaposleniciController", "Šifrarnik")
component(8.8,  7.15, 2.4, 1.3, "Razor Views", "Index/Details/\nCreate/Edit")
component(11.4, 7.15, 2.1, 1.3, "ViewModels", "Lokacija/Zaposlenik\nZaliha…")

# Business
layer(0.3, 4.9, 13.4, 1.9, "Poslovni sloj  —  ChainTrack.BLL", LAYER_COLORS["business"])
component(0.7,  5.05, 2.6, 1.3, "ILokacijaService", "LokacijaService")
component(3.5,  5.05, 2.6, 1.3, "IZaposlenikService", "ZaposlenikService")
component(6.3,  5.05, 2.6, 1.3, "ISifrarnikService", "SifrarnikService")
component(9.1,  5.05, 2.3, 1.3, "Validation", "OibValidator\n(ISO 7064 MOD 11,10)")
component(11.6, 5.05, 1.9, 1.3, "Exceptions", "Validacija/\nPoslovnoPravilo/\nNijePronadeno")

# DAL
layer(0.3, 2.8, 13.4, 1.9, "Sloj za pristup podacima (DAL)  —  ChainTrack.DAL (EF Core)", LAYER_COLORS["dal"])
component(0.7,  2.95, 2.5, 1.3, "ILokacijaRepository", "LokacijaRepository")
component(3.4,  2.95, 2.7, 1.3, "IZalihaSastojkaRepository", "ZalihaSastojkaRepository")
component(6.3,  2.95, 2.5, 1.3, "IZaposlenikRepository", "ZaposlenikRepository")
component(9.0,  2.95, 2.3, 1.3, "ISifrarnikRepository", "SifrarnikRepository")
component(11.5, 2.95, 2.0, 1.3, "ChainTrackDbContext", "+ DatabaseSeeder")

# Domain
layer(0.3, 0.9, 9.0, 1.7, "Domenski sloj  —  ChainTrack.Domain", LAYER_COLORS["domain"])
component(0.7,  1.05, 2.0, 1.2, "Entities", "Lokacija, Zaposlenik,\nSastojak, ZalihaSastojka…")
component(2.9,  1.05, 1.9, 1.2, "Enums", "StatusZaposlenika\nTipObjekta\nUlogaKorisnika")
component(5.0,  1.05, 1.9, 1.2, "Common", "PagedResult<T>")
component(7.1,  1.05, 2.0, 1.2, "Šifrarnici", "TipZaposlenika,\nKategorijaSastojka,\nKategorijaPrihoda")

# External
layer(9.6, 0.9, 4.1, 1.7, "Vanjski sustavi", LAYER_COLORS["external"])
component(9.9,  1.05, 1.7, 1.2, "PostgreSQL", "produkcija")
component(11.8, 1.05, 1.7, 1.2, "SQLite", "dev/testovi")

# === Strelice ovisnosti (uses) ===
# Presentation -> BLL
arrow(4.5, 7.15, 2.0, 6.35, "uses")
arrow(7.3, 7.15, 4.8, 6.35, "uses")
arrow(7.3, 7.15, 7.6, 6.35, "uses")
# BLL -> DAL
arrow(2.0, 5.05, 2.0, 4.25, "uses")
arrow(4.8, 5.05, 4.8, 4.25, "uses")
arrow(7.6, 5.05, 7.6, 4.25, "uses")
arrow(10.2, 5.05, 10.2, 4.25, "uses")
# DAL -> Domain
arrow(2.0, 2.95, 2.0, 2.25, "")
arrow(4.8, 2.95, 4.8, 2.25, "")
arrow(7.6, 2.95, 5.9, 2.25, "")
arrow(10.2, 2.95, 8.1, 2.25, "")
# BLL -> Domain (entiteti se dijele kroz cijeli stack)
arrow(11.0, 5.05, 6.0, 2.25, "domain model", color="#666", offset=(0,0.1))
# DbContext -> Postgres/Sqlite
arrow(12.5, 2.95, 10.75, 2.25, "EF Core")
arrow(12.5, 2.95, 12.65, 2.25, "EF Core")

# Legenda
legend_x, legend_y = 0.4, 0.15
ax.add_patch(FancyBboxPatch((legend_x, legend_y), 6.2, 0.55,
                            boxstyle="round,pad=0.02", linewidth=0.8,
                            edgecolor="#888", facecolor="#fafafa"))
ax.text(legend_x + 0.15, legend_y + 0.32,
        "Strelica  ─▶  označava smjer ovisnosti (gornji sloj koristi sučelja donjeg sloja; DI u Program.cs).",
        fontsize=8, color="#333")
ax.text(legend_x + 0.15, legend_y + 0.12,
        "Test projekt ChainTrack.Tests testira sve slojeve (xUnit + Moq + EF Core In-Memory/SQLite).",
        fontsize=8, color="#333")

plt.tight_layout()
out = __file__.replace("generate_arhitektura.py", "arhitektura.png")
plt.savefig(out, dpi=180, bbox_inches='tight', facecolor='white')
print("Spremljeno:", out)
