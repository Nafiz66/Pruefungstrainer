using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data.Configurations;

public class FrageConfiguration : IEntityTypeConfiguration<Frage>
{
    public void Configure(EntityTypeBuilder<Frage> builder)
    {
        builder.ToTable("Fragen");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Fragetext)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.Typ)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(f => f.Themengebiet)
            .WithMany(t => t.Fragen)
            .HasForeignKey(f => f.ThemengebietId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Antwortoptionen)
            .WithOne(a => a.Frage)
            .HasForeignKey(a => a.FrageId)
            // Cascade: Antwortoptionen existieren nur im Kontext ihrer Frage.
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Ergebnisdetails)
            .WithOne(d => d.Frage)
            .HasForeignKey(d => d.FrageId)
            // Restrict: Prüfungsergebnisse müssen auch dann nachvollziehbar bleiben,
            // wenn eine Frage nachträglich gelöscht werden soll (dann muss die Frage
            // z.B. stattdessen deaktiviert statt gelöscht werden).
            .OnDelete(DeleteBehavior.Restrict);
    }
}
