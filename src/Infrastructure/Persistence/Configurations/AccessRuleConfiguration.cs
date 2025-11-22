using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the AccessRule entity.
/// </summary>
public class AccessRuleConfiguration : IEntityTypeConfiguration<AccessRule>
{
    public void Configure(EntityTypeBuilder<AccessRule> builder)
    {
        // Table configuration
        builder.ToTable("AccessRules");

        // Primary key with auto-generation
        builder.HasKey(ar => ar.Id);
        builder.Property(ar => ar.Id)
            .ValueGeneratedOnAdd();

        // Ignore the complex properties - we'll map them as separate columns
        builder.Ignore(ar => ar.TimeRange);
        builder.Ignore(ar => ar.ValidityPeriod);

        // Map TimeRange properties as individual nullable columns
        builder.Property<TimeOnly?>("TimeRangeStartTime")
            .HasColumnName("TimeRangeStartTime");

        builder.Property<TimeOnly?>("TimeRangeEndTime")
            .HasColumnName("TimeRangeEndTime");

        // Map DateRange properties as individual nullable columns
        builder.Property<DateOnly?>("ValidityStartDate")
            .HasColumnName("ValidityStartDate");

        builder.Property<DateOnly?>("ValidityEndDate")
            .HasColumnName("ValidityEndDate");

        // Foreign keys
        builder.Property(ar => ar.TenantId)
            .IsRequired();

        builder.Property(ar => ar.ControlPointId)
            .IsRequired();

        // Relationships
        builder.HasOne(ar => ar.Tenant)
            .WithMany()
            .HasForeignKey(ar => ar.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ar => ar.ControlPoint)
            .WithMany(cp => cp.AccessRules)
            .HasForeignKey(ar => ar.ControlPointId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many relationship with Role (unidirectional from AccessRule)
        builder.HasMany(ar => ar.Roles)
            .WithMany()
            .UsingEntity(
                "AccessRuleRoles",
                l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId"),
                r => r.HasOne(typeof(AccessRule)).WithMany().HasForeignKey("AccessRuleId"),
                j => j.HasKey("AccessRuleId", "RoleId"));

        // Indexes
        builder.HasIndex(ar => ar.TenantId);
    }
}
