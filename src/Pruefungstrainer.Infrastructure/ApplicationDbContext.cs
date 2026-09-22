using Microsoft.EntityFrameworkCore;
using PruefungstrainingPlattform.Core.Entities;

namespace PruefungstrainingPlattform.Infrastructure.Data;

/// <summary>
/// Der EF-Core-DbContext der Prüfungstrainingsplattform. Bündelt alle
/// <see cref="DbSet{TEntity}"/>-Eigenschaften und lädt die Fluent-API-Konfigurationen
/// aus dem Unterordner <c>Configurations</c>.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Benutzer => Set<ApplicationUser>();

    public DbSet<Themengebiet> Themengebiete => Set<Themengebiet>();

    public DbSet<Lernpfad> Lernpfade => Set<Lernpfad>();

    public DbSet<LernpfadElement> LernpfadElemente => Set<LernpfadElement>();

    public DbSet<Karteikarte> Karteikarten => Set<Karteikarte>();

    public DbSet<Frage> Fragen => Set<Frage>();

    public DbSet<Antwortoption> Antwortoptionen => Set<Antwortoption>();

    public DbSet<KarteikartenFortschritt> KarteikartenFortschritte => Set<KarteikartenFortschritt>();

    public DbSet<Pruefungsergebnis> Pruefungsergebnisse => Set<Pruefungsergebnis>();

    public DbSet<PruefungsergebnisFrage> PruefungsergebnisFragen => Set<PruefungsergebnisFrage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Lädt automatisch alle Klassen, die IEntityTypeConfiguration<T> implementieren,
        // aus diesem Assembly (siehe Ordner "Configurations"). So bleibt OnModelCreating
        // übersichtlich, auch wenn die Anzahl der Entitäten wächst.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
