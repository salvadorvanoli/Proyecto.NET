using Domain.Constants;
using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table configuration
        builder.ToTable("Users");

        // Primary key with auto-generation
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // Properties configuration
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(DomainConstants.StringLengths.EmailMaxLength);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(DomainConstants.StringLengths.PasswordHashMaxLength);

        // Complex type configuration for PersonalData
        builder.ComplexProperty(u => u.PersonalData, pd =>
        {
            pd.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(DomainConstants.StringLengths.FirstNameMaxLength)
                .HasColumnName("FirstName");

            pd.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(DomainConstants.StringLengths.LastNameMaxLength)
                .HasColumnName("LastName");

            pd.Property(p => p.BirthDate)
                .IsRequired()
                .HasColumnName("BirthDate");
        });

        // Foreign keys
        builder.Property(u => u.TenantId)
            .IsRequired();

        builder.Property(u => u.CredentialId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Credential)
            .WithOne(c => c.User)
            .HasForeignKey<User>(u => u.CredentialId)
            .OnDelete(DeleteBehavior.SetNull);

        // Many-to-many relationship with Role (unidirectional from User)
        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity(
                "UserRoles",
                l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("UserId", "RoleId");
                    j.ToTable("UserRoles");
                    j.HasIndex("UserId");
                    j.HasIndex("RoleId");
                });

        // Indexes
        builder.HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_Users_Email_TenantId");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        builder.HasIndex(u => u.CredentialId)
            .HasDatabaseName("IX_Users_CredentialId");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");
    }
}
