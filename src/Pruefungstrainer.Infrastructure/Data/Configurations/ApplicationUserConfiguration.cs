using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Benutzer");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Kein Benutzer darf die gleiche E-Mail-Adresse doppelt verwenden (z.B. für den Login).
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Enum als lesbarer String statt als Zahl speichern – erleichtert das manuelle
        // Prüfen der SQLite-Datenbank während der Entwicklung/Prüfung.
        builder.Property(u => u.Rolle)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasMany(u => u.KarteikartenFortschritte)
            .WithOne(f => f.ApplicationUser)
            .HasForeignKey(f => f.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Pruefungsergebnisse)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
