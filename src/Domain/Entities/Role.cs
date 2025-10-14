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

    // Navigation properties
    public virtual ICollection<User> Users { get; protected set; } = new List<User>();
    public virtual ICollection<AccessRule> AccessRules { get; protected set; } = new List<AccessRule>();

    protected Role() : base()
    {
        Name = string.Empty;
    }

    public Role(int tenantId, string name) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrWhiteSpace, nameof(name)), nameof(name));

        Name = name.Trim();
    }

    /// <summary>
    /// Updates the role name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrWhiteSpace, nameof(name)), nameof(name));

        Name = name.Trim();
        UpdateTimestamp();
    }
}

