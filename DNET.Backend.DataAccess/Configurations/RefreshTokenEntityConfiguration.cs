using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Token)
            .HasColumnName("token")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(e => e.IsRevoked)
            .HasColumnName("is_revoked")
            .IsRequired();

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany() // one User → many RefreshTokens
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}