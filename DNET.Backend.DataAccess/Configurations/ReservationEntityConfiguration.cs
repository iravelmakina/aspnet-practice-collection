using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class ReservationEntityConfiguration : IEntityTypeConfiguration<ReservationEntity>
{
    public void Configure(EntityTypeBuilder<ReservationEntity> builder)
    {
        builder.ToTable("reservation");
        // HasCheckConstraint("CK_Reservation_Time", "StartTime < EndTime");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Uid)
            .HasColumnName("uid")
            .IsRequired();
        builder.HasIndex(e => e.Uid);

        builder.Property(e => e.StartTime)
            .IsRequired();
        builder.Property(e => e.EndTime)
            .IsRequired();

        builder.HasOne(e => e.Client)
            .WithMany(e => e.Reservations)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Table)
            .WithMany(e => e.Reservations)
            .HasForeignKey(e => e.TableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}