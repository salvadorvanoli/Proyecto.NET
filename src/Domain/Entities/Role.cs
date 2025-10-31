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
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, nameof(name)), nameof(name));

        Name = name.Trim();
    }

    /// <summary>
    /// Updates the role name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, nameof(name)), nameof(name));

        Name = name.Trim();
        UpdateTimestamp();
    }
}

