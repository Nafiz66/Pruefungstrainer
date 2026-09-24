using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data.Configurations;

public class PruefungsergebnisConfiguration : IEntityTypeConfiguration<Pruefungsergebnis>
{
    public void Configure(EntityTypeBuilder<Pruefungsergebnis> builder)
    {
        builder.ToTable("Pruefungsergebnisse");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.DurchgefuehrtAm)
            .IsRequired();

        // Erleichtert Abfragen wie "alle Ergebnisse eines Benutzers, neueste zuerst".
        builder.HasIndex(p => new { p.ApplicationUserId, p.DurchgefuehrtAm });

        builder.HasMany(p => p.Details)
            .WithOne(d => d.Pruefungsergebnis)
            .HasForeignKey(d => d.PruefungsergebnisId)
            // Cascade: Detailantworten existieren nur im Kontext ihres Prüfungsergebnisses.
            .OnDelete(DeleteBehavior.Cascade);

        // Die Beziehungen zu ApplicationUser und Lernpfad werden zentral in deren
        // jeweiliger Configuration-Klasse definiert, um Doppelkonfiguration zu vermeiden.
    }
}
