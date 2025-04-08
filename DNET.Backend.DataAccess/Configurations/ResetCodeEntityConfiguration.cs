using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class ResetCodeEntityConfiguration : IEntityTypeConfiguration<ResetCodeEntity>
{
    public void Configure(EntityTypeBuilder<ResetCodeEntity> builder)
    {
        builder.ToTable("reset_codes");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.HasOne(e => e.User)
            .WithMany() // one User → many ResetCodes
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}