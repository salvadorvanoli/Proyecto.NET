using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.ToTable("News");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedOnAdd();

        builder.Property(n => n.Title).IsRequired().HasMaxLength(DomainConstants.StringLengths.TitleMaxLength);
        builder.Property(n => n.Content).IsRequired();
        builder.Property(n => n.PublishDate).IsRequired();
        builder.Property(n => n.TenantId).IsRequired();

        builder.HasOne(n => n.Tenant).WithMany().HasForeignKey(n => n.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(n => n.TenantId);
        builder.HasIndex(n => n.PublishDate);
    }
}

public class SpaceTypeConfiguration : IEntityTypeConfiguration<SpaceType>
{
    public void Configure(EntityTypeBuilder<SpaceType> builder)
    {
        builder.ToTable("SpaceTypes");
        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id).ValueGeneratedOnAdd();

        builder.Property(st => st.Name).IsRequired().HasMaxLength(DomainConstants.StringLengths.SpaceTypeNameMaxLength);
        builder.Property(st => st.TenantId).IsRequired();

        builder.HasOne(st => st.Tenant).WithMany().HasForeignKey(st => st.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(st => st.TenantId);
        builder.HasIndex(st => new { st.Name, st.TenantId }).IsUnique();
    }
}

public class ControlPointConfiguration : IEntityTypeConfiguration<ControlPoint>
{
    public void Configure(EntityTypeBuilder<ControlPoint> builder)
    {
        builder.ToTable("ControlPoints");
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id).ValueGeneratedOnAdd();

        builder.Property(cp => cp.Name).IsRequired().HasMaxLength(DomainConstants.StringLengths.ControlPointNameMaxLength);
        builder.Property(cp => cp.TenantId).IsRequired();
        builder.Property(cp => cp.SpaceId).IsRequired();

        builder.HasOne(cp => cp.Tenant).WithMany().HasForeignKey(cp => cp.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(cp => cp.Space).WithMany(s => s.ControlPoints).HasForeignKey(cp => cp.SpaceId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cp => cp.TenantId);
        builder.HasIndex(cp => cp.SpaceId);
        builder.HasIndex(cp => new { cp.Name, cp.SpaceId }).IsUnique();
    }
}

public class AccessEventConfiguration : IEntityTypeConfiguration<AccessEvent>
{
    public void Configure(EntityTypeBuilder<AccessEvent> builder)
    {
        builder.ToTable("AccessEvents");
        builder.HasKey(ae => ae.Id);
        builder.Property(ae => ae.Id).ValueGeneratedOnAdd();

        builder.Property(ae => ae.EventDateTime).IsRequired();
        builder.Property(ae => ae.Result).IsRequired().HasConversion<int>();
        builder.Property(ae => ae.TenantId).IsRequired();
        builder.Property(ae => ae.ControlPointId).IsRequired();
        builder.Property(ae => ae.UserId).IsRequired();

        builder.HasOne(ae => ae.Tenant).WithMany().HasForeignKey(ae => ae.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(ae => ae.ControlPoint).WithMany(cp => cp.AccessEvents).HasForeignKey(ae => ae.ControlPointId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ae => ae.User).WithMany(u => u.AccessEvents).HasForeignKey(ae => ae.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ae => ae.TenantId);
        builder.HasIndex(ae => ae.EventDateTime);
        builder.HasIndex(ae => new { ae.UserId, ae.EventDateTime });
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(DomainConstants.StringLengths.RoleNameMaxLength);
        builder.Property(r => r.TenantId).IsRequired();

        builder.HasOne(r => r.Tenant).WithMany().HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => new { r.Name, r.TenantId }).IsUnique();
    }
}

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("Credentials");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.IssueDate).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.UserId).IsRequired();

        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => new { c.UserId, c.IsActive });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedOnAdd();

        builder.Property(n => n.Title).IsRequired().HasMaxLength(DomainConstants.StringLengths.TitleMaxLength);
        builder.Property(n => n.Message).IsRequired();
        builder.Property(n => n.SentDateTime).IsRequired();
        builder.Property(n => n.IsRead).IsRequired();
        builder.Property(n => n.TenantId).IsRequired();
        builder.Property(n => n.UserId).IsRequired();

        builder.HasOne(n => n.Tenant).WithMany().HasForeignKey(n => n.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.TenantId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => n.SentDateTime);
    }
}
