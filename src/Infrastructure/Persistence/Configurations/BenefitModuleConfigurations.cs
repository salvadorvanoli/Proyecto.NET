using Domain.Constants;
using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BenefitTypeConfiguration : IEntityTypeConfiguration<BenefitType>
{
    public void Configure(EntityTypeBuilder<BenefitType> builder)
    {
        builder.ToTable("BenefitTypes");
        builder.HasKey(bt => bt.Id);
        builder.Property(bt => bt.Id).ValueGeneratedOnAdd();

        builder.Property(bt => bt.Name).IsRequired().HasMaxLength(DomainConstants.StringLengths.NameMaxLength);
        builder.Property(bt => bt.Description).IsRequired().HasMaxLength(DomainConstants.StringLengths.DescriptionMaxLength);
        builder.Property(bt => bt.TenantId).IsRequired();

        builder.HasOne(bt => bt.Tenant).WithMany().HasForeignKey(bt => bt.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bt => bt.TenantId);
        builder.HasIndex(bt => new { bt.Name, bt.TenantId }).IsUnique();
    }
}

public class BenefitConfiguration : IEntityTypeConfiguration<Benefit>
{
    public void Configure(EntityTypeBuilder<Benefit> builder)
    {
        builder.ToTable("Benefits");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedOnAdd();

        builder.Property(b => b.Quotas).IsRequired();
        builder.Property(b => b.Quantity).IsRequired();
        builder.Property(b => b.TenantId).IsRequired();
        builder.Property(b => b.BenefitTypeId).IsRequired();
        builder.Property(b => b.Active).IsRequired().HasDefaultValue(true);

        // Ignore the complex property - we'll map it as separate columns
        builder.Ignore(b => b.ValidityPeriod);

        // Properties for ValidityPeriod (nullable)
        builder.Property<DateOnly?>("ValidityStartDate")
            .HasColumnName("ValidityStartDate");

        builder.Property<DateOnly?>("ValidityEndDate")
            .HasColumnName("ValidityEndDate");

        builder.HasOne(b => b.Tenant).WithMany().HasForeignKey(b => b.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.BenefitType).WithMany().HasForeignKey(b => b.BenefitTypeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.TenantId);
        builder.HasIndex(b => b.BenefitTypeId);
    }
}

public class ConsumptionConfiguration : IEntityTypeConfiguration<Consumption>
{
    public void Configure(EntityTypeBuilder<Consumption> builder)
    {
        builder.ToTable("Consumptions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Amount).IsRequired();
        builder.Property(c => c.ConsumptionDateTime).IsRequired();
        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.UsageId).IsRequired();

        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.UsageId);
    }
}

public class UsageConfiguration : IEntityTypeConfiguration<Usage>
{
    public void Configure(EntityTypeBuilder<Usage> builder)
    {
        builder.ToTable("Usages");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.Quantity).IsRequired();
        builder.Property(u => u.TenantId).IsRequired();
        builder.Property(u => u.BenefitId).IsRequired();
        builder.Property(u => u.UserId).IsRequired();

        builder.HasOne(u => u.Tenant).WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(u => u.Benefit).WithMany().HasForeignKey(u => u.BenefitId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.BenefitId);
        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.Quantity);
    }
}
