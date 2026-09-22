using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruefungstrainingPlattform.Core.Entities;

namespace PruefungstrainingPlattform.Infrastructure.Data.Configurations;

public class KarteikarteConfiguration : IEntityTypeConfiguration<Karteikarte>
{
    public void Configure(EntityTypeBuilder<Karteikarte> builder)
    {
        builder.ToTable("Karteikarten");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Frage)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(k => k.Antwort)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(k => k.Themengebiet)
            .WithMany(t => t.Karteikarten)
            .HasForeignKey(k => k.ThemengebietId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
