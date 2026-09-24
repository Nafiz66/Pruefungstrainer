using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data.Configurations;

public class ThemengebietConfiguration : IEntityTypeConfiguration<Themengebiet>
{
    public void Configure(EntityTypeBuilder<Themengebiet> builder)
    {
        builder.ToTable("Themengebiete");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        // Themengebietsnamen sollen eindeutig sein, um doppelte Anlage zu vermeiden.
        builder.HasIndex(t => t.Name)
            .IsUnique();

        // Die 1:n-Beziehungen zu Lernpfad/Karteikarte/Frage werden bewusst dort
        // konfiguriert (nicht hier), um Duplikate in der Konfiguration zu vermeiden.
    }
}
