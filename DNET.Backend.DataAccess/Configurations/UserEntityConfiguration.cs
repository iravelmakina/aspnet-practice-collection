using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNET.Backend.DataAccess.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn()
            .IsRequired();
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .HasColumnName("username")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(256);

        builder.Property(e => e.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(256);
        
        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.PasswordSalt)
            .HasColumnName("password_salt")
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
        
        builder.Property(e => e.LoginProvider)
            .HasColumnName("login_provider")
            .HasMaxLength(256)
            .IsRequired(false);
        
        builder.Property(e => e.Role)
            .HasColumnName("role")
            .HasMaxLength(256)
            .IsRequired();
    }
}