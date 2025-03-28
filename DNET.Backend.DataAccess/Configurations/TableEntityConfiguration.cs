using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class TableEntityConfiguration : IEntityTypeConfiguration<TableEntity>
{
    public void Configure(EntityTypeBuilder<TableEntity> builder)
    {
        builder.ToTable("tables");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();
        
        builder.Property(t => t.Number)
            .HasColumnName("number")
            .IsRequired();
        
        builder.Property(t => t.Capacity)
            .HasColumnName("capacity")
            .IsRequired();
        
        builder.Property(t => t.LocationId)
            .HasColumnName("location_id");

        builder.HasOne(t => t.Location)
            .WithMany(l => l.Tables) // one Location → many Tables
            .HasForeignKey(t => t.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}