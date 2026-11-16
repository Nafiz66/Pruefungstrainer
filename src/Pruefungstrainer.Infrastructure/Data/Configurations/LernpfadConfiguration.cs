using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data.Configurations;

public class LernpfadConfiguration : IEntityTypeConfiguration<Lernpfad>
{
    public void Configure(EntityTypeBuilder<Lernpfad> builder)
    {
        builder.ToTable("Lernpfade");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Titel)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(l => l.Themengebiet)
            .WithMany(t => t.Lernpfade)
            .HasForeignKey(l => l.ThemengebietId)
            // Restrict: Ein Themengebiet mit vorhandenen Lernpfaden darf nicht
            // gelöscht werden, ohne dass die Lernpfade vorher entfernt/umgehängt wurden.
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Pruefungsergebnisse)
            .WithOne(p => p.Lernpfad)
            .HasForeignKey(p => p.LernpfadId)
            // SetNull: Wird ein Lernpfad gelöscht, bleiben bereits erzielte
            // Prüfungsergebnisse als historische Daten erhalten (LernpfadId wird null).
            .OnDelete(DeleteBehavior.SetNull);
    }
}
