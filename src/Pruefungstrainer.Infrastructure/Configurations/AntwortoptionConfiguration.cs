using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruefungstrainingPlattform.Core.Entities;

namespace PruefungstrainingPlattform.Infrastructure.Data.Configurations;

public class AntwortoptionConfiguration : IEntityTypeConfiguration<Antwortoption>
{
    public void Configure(EntityTypeBuilder<Antwortoption> builder)
    {
        builder.ToTable("Antwortoptionen");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Text)
            .IsRequired()
            .HasMaxLength(500);

        // Die Beziehung zu Frage (inkl. Cascade-Delete) wird zentral in
        // FrageConfiguration definiert, um Doppelkonfiguration zu vermeiden.
    }
}
