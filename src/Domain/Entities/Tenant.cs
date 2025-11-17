using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a tenant in the multi-tenant system.
/// Root entity that owns all other entities in the system.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Name of the tenant organization.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Primary brand color in hex format (e.g., #007bff).
    /// </summary>
    public string PrimaryColor { get; protected set; }

    /// <summary>
    /// Secondary brand color in hex format (e.g., #6c757d).
    /// </summary>
    public string SecondaryColor { get; protected set; }

    /// <summary>
    /// Accent brand color in hex format (e.g., #ffc107).
    /// </summary>
    public string AccentColor { get; protected set; }

    /// <summary>
    /// Logo file path or URL.
    /// </summary>
    public string? Logo { get; protected set; }

    /// <summary>
    /// Timestamp when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Timestamp when the tenant was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    // Note: No navigation properties to avoid performance issues
    // Each entity references TenantId instead of Tenant having collections

    protected Tenant()
    {
        Name = string.Empty;
        PrimaryColor = "#007bff";
        SecondaryColor = "#6c757d";
        AccentColor = "#ffc107";
        Logo = null;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Tenant(string name, string primaryColor, string secondaryColor, string accentColor, string? logo = null) : this()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Tenant name"),
                nameof(name));

        var trimmedName = name.Trim();

        if (trimmedName.Length < DomainConstants.StringLengths.NameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Tenant name", DomainConstants.StringLengths.NameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.NameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Tenant name", DomainConstants.StringLengths.NameMaxLength),
                nameof(name));

        Name = trimmedName;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        AccentColor = accentColor;
        Logo = logo;
    }

    /// <summary>
    /// Updates the tenant name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Tenant name"),
                nameof(name));

        var trimmedName = name.Trim();

        if (trimmedName.Length < DomainConstants.StringLengths.NameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Tenant name", DomainConstants.StringLengths.NameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.NameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Tenant name", DomainConstants.StringLengths.NameMaxLength),
                nameof(name));

        Name = trimmedName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the tenant branding colors.
    /// </summary>
    public void UpdateBranding(string primaryColor, string secondaryColor, string accentColor, string? logo = null)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        AccentColor = accentColor;
        Logo = logo;
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Tenant other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id == DomainConstants.NumericValidation.TransientEntityId || other.Id == DomainConstants.NumericValidation.TransientEntityId)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, GetType());
    }
}

