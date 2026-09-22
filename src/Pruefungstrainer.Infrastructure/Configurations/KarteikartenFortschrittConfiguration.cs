using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruefungstrainingPlattform.Core.Entities;

namespace PruefungstrainingPlattform.Infrastructure.Data.Configurations;

public class KarteikartenFortschrittConfiguration : IEntityTypeConfiguration<KarteikartenFortschritt>
{
    public void Configure(EntityTypeBuilder<KarteikartenFortschritt> builder)
    {
        builder.ToTable("KarteikartenFortschritte");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Pro Benutzer darf es zu jeder Karteikarte nur genau einen Fortschrittsdatensatz geben.
        builder.HasIndex(f => new { f.ApplicationUserId, f.KarteikarteId })
            .IsUnique();

        builder.HasOne(f => f.Karteikarte)
            .WithMany(k => k.Fortschritte)
            .HasForeignKey(f => f.KarteikarteId)
            // Cascade: Wird eine Karteikarte gelöscht, sind die dazugehörigen
            // Fortschrittsdaten ohne Aussagekraft und werden mitgelöscht.
            .OnDelete(DeleteBehavior.Cascade);

        // Die Beziehung zu ApplicationUser (inkl. Cascade-Delete) wird zentral in
        // ApplicationUserConfiguration definiert, um Doppelkonfiguration zu vermeiden.
    }
}
