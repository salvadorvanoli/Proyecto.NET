using Domain.Constants;
using Domain.DataTypes;

namespace Domain.Entities;

/// <summary>
/// Represents an access rule that defines when and who can access a control point.
/// </summary>
public class AccessRule : BaseEntity
{
    /// <summary>
    /// Time range during which the rule is active (can be null for 24/7 access).
    /// </summary>
    public TimeRange? TimeRange { get; protected set; }

    /// <summary>
    /// Date range during which the rule is valid (can be null for permanent validity).
    /// </summary>
    public DateRange? ValidityPeriod { get; protected set; }

    // Navigation properties
    public virtual ICollection<Role> Roles { get; protected set; } = new List<Role>();

    protected AccessRule() : base()
    {
    }

    public AccessRule(int tenantId, TimeRange? timeRange = null, DateRange? validityPeriod = null)
        : base(tenantId)
    {
        TimeRange = timeRange;
        ValidityPeriod = validityPeriod;
    }

    /// <summary>
    /// Updates the time range for this access rule.
    /// </summary>
    public void UpdateTimeRange(TimeRange? timeRange)
    {
        TimeRange = timeRange;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the validity period for this access rule.
    /// </summary>
    public void UpdateValidityPeriod(DateRange? validityPeriod)
    {
        ValidityPeriod = validityPeriod;
        UpdateTimestamp();
    }

    /// <summary>
    /// Adds a role to this access rule.
    /// </summary>
    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (role.TenantId != TenantId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBelongToSameTenant, "Role"),
                nameof(role));

        if (!Roles.Contains(role))
        {
            Roles.Add(role);
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Removes a role from this access rule.
    /// </summary>
    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (Roles.Remove(role))
        {
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Checks if the rule is currently active based on time and date constraints.
    /// </summary>
    public bool IsActiveAt(DateTime dateTime)
    {
        // Check date validity
        if (ValidityPeriod.HasValue && !ValidityPeriod.Value.Contains(dateTime))
            return false;

        // Check time range
        if (TimeRange.HasValue && !TimeRange.Value.Contains(dateTime))
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a user with specific roles can access based on this rule.
    /// </summary>
    public bool AllowsAccess(IEnumerable<Role> userRoles)
    {
        if (userRoles == null)
            return false;

        return Roles.Any(role => userRoles.Contains(role));
    }
}

