using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class LocationEntityConfiguration : IEntityTypeConfiguration<LocationEntity>
{
    public void Configure(EntityTypeBuilder<LocationEntity> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasData(
            new LocationEntity() { Id = 1, Name = "Main Hall" },
            new LocationEntity() { Id = 2, Name = "Patio" },
            new LocationEntity() { Id = 3, Name = "Private Room" },
            new LocationEntity() { Id = 4, Name = "Bar" },
            new LocationEntity() { Id = 5, Name = "Terrace" },
            new LocationEntity() { Id = 6, Name = "Garden" },
            new LocationEntity() { Id = 7, Name = "Rooftop" }
        );

        builder.HasMany(l => l.Tables)
            .WithOne(t => t.Location) // many Tables → one Location
            .HasForeignKey(t => t.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
