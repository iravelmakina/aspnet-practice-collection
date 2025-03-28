using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class ReservationDetailEntityConfiguration : IEntityTypeConfiguration<ReservationDetailEntity>
{
    public void Configure(EntityTypeBuilder<ReservationDetailEntity> builder)
    {
        builder.ToTable("reservation_detail");
        
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Reservation)
            .WithOne(e => e.ReservationDetail)  // One-to-One relationship
            .HasForeignKey<ReservationDetailEntity>(e => e.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(e => e.ReservationType)
            .HasConversion(
                v => v.ToString(), // Convert enum value to string when saving to DB
                v => (ReservationDetailEntity.ReservationTypeEnum)Enum.Parse(
                    typeof(ReservationDetailEntity.ReservationTypeEnum),
                    v) // Convert back from string to enum when loading from DB
            );
        
        builder.Property(e => e.SpecialRequests)
            .HasColumnName("special_requests")
            .IsRequired();
    }
}