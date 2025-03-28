using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class HostEntityConfiguration : IEntityTypeConfiguration<HostEntity>
{
    public void Configure(EntityTypeBuilder<HostEntity> builder)
    {
        builder.ToTable("hosts");
        
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();
        
        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(h => h.TableId)
            .HasColumnName("table_id");

        builder.HasOne(h => h.Table)
            .WithOne(t => t.Host) // one Host → one Table
            .HasForeignKey<HostEntity>(t => t.TableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
