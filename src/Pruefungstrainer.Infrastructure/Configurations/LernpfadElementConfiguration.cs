using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruefungstrainingPlattform.Core.Entities;

namespace PruefungstrainingPlattform.Infrastructure.Data.Configurations;

public class LernpfadElementConfiguration : IEntityTypeConfiguration<LernpfadElement>
{
    public void Configure(EntityTypeBuilder<LernpfadElement> builder)
    {
        builder.ToTable("LernpfadElemente", t => t.HasCheckConstraint(
            "CK_LernpfadElement_GenauEinInhalt",
            // Stellt sicher, dass passend zum Typ genau eine der beiden Referenzen gesetzt ist.
            "(Typ = 'Karteikarte' AND KarteikarteId IS NOT NULL AND FrageId IS NULL) " +
            "OR (Typ = 'Frage' AND FrageId IS NOT NULL AND KarteikarteId IS NULL)"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Typ)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Innerhalb eines Lernpfads darf jede Reihenfolgeposition nur einmal vergeben werden.
        builder.HasIndex(e => new { e.LernpfadId, e.Reihenfolge })
            .IsUnique();

        builder.HasOne(e => e.Lernpfad)
            .WithMany(l => l.Elemente)
            .HasForeignKey(e => e.LernpfadId)
            // Cascade: Wird ein Lernpfad gelöscht, verlieren seine Elemente ihren Sinn
            // und werden automatisch mitgelöscht.
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Karteikarte)
            .WithMany(k => k.LernpfadElemente)
            .HasForeignKey(e => e.KarteikarteId)
            // Restrict: Eine Karteikarte, die noch in einem Lernpfad verwendet wird,
            // kann nicht einfach gelöscht werden.
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Frage)
            .WithMany(f => f.LernpfadElemente)
            .HasForeignKey(e => e.FrageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
