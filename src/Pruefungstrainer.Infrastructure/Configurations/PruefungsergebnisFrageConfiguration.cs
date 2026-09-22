using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruefungstrainingPlattform.Core.Entities;

namespace PruefungstrainingPlattform.Infrastructure.Data.Configurations;

public class PruefungsergebnisFrageConfiguration : IEntityTypeConfiguration<PruefungsergebnisFrage>
{
    public void Configure(EntityTypeBuilder<PruefungsergebnisFrage> builder)
    {
        builder.ToTable("PruefungsergebnisFragen");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.GegebeneAntwort)
            .HasMaxLength(1000);

        // Jede Frage darf innerhalb eines einzelnen Prüfungsergebnisses nur einmal vorkommen.
        builder.HasIndex(d => new { d.PruefungsergebnisId, d.FrageId })
            .IsUnique();

        // Die Beziehungen zu Pruefungsergebnis (Cascade) und Frage (Restrict) werden
        // zentral in deren jeweiliger Configuration-Klasse definiert.
    }
}
