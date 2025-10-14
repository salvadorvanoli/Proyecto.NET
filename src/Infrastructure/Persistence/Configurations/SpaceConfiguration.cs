using Domain.Constants;
using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Space entity.
/// </summary>
public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        // Table configuration
        builder.ToTable("Spaces");

        // Primary key with auto-generation
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        // Properties configuration
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(DomainConstants.StringLengths.NameMaxLength);

        // Complex type configuration for Location
        builder.ComplexProperty(s => s.Location, loc =>
        {
            loc.Property(l => l.Street)
                .IsRequired()
                .HasMaxLength(DomainConstants.StringLengths.StreetMaxLength)
                .HasColumnName("Street");

            loc.Property(l => l.Number)
                .IsRequired()
                .HasMaxLength(DomainConstants.StringLengths.NumberMaxLength)
                .HasColumnName("Number");

            loc.Property(l => l.City)
                .IsRequired()
                .HasMaxLength(DomainConstants.StringLengths.CityMaxLength)
                .HasColumnName("City");

            loc.Property(l => l.Country)
                .IsRequired()
                .HasMaxLength(DomainConstants.StringLengths.CountryMaxLength)
                .HasColumnName("Country");
        });

        // Foreign keys
        builder.Property(s => s.TenantId)
            .IsRequired();

        builder.Property(s => s.SpaceTypeId)
            .IsRequired();

        // Relationships
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SpaceType)
            .WithMany(st => st.Spaces)
            .HasForeignKey(s => s.SpaceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.SpaceTypeId);
        builder.HasIndex(s => new { s.Name, s.TenantId });
    }
}
