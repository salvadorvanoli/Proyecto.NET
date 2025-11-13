using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users for access control.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Name of the role.
    /// </summary>
    public string Name { get; protected set; }

    protected Role() : base()
    {
        Name = string.Empty;
    }

    public Role(int tenantId, string name) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"), nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.RoleNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.RoleNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.RoleNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.RoleNameMaxLength),
                nameof(name));

        Name = trimmedName;
    }

    /// <summary>
    /// Updates the role name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"), nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.RoleNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.RoleNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.RoleNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.RoleNameMaxLength),
                nameof(name));

        Name = trimmedName;
        UpdateTimestamp();
    }
}

