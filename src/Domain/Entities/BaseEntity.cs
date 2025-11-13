using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Base class for all domain entities with common properties and behavior.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Reference to the tenant that owns this entity.
    /// </summary>
    public int TenantId { get; protected set; }

    /// <summary>
    /// Navigation property to the tenant.
    /// </summary>
    public virtual Tenant Tenant { get; protected set; } = null!;

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Timestamp when the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    /// <summary>
    /// Protected parameterless constructor for EF Core.
    /// Should not be used directly in domain code.
    /// </summary>
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor with tenant validation for domain entities.
    /// </summary>
    /// <param name="tenantId">The tenant identifier. Must be greater than zero.</param>
    /// <exception cref="ArgumentException">Thrown when tenantId is invalid.</exception>
    protected BaseEntity(int tenantId) : this()
    {
        if (tenantId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID"),
                nameof(tenantId));

        TenantId = tenantId;
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp.
    /// Should only be called by domain methods, not externally.
    /// </summary>
    protected virtual void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks equality based on Id for entities with the same type.
    /// Two entities are equal if they have the same Id and are of the same type.
    /// Transient entities (Id == 0) are only equal if they are the same reference.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == DomainConstants.NumericValidation.TransientEntityId || other.Id == DomainConstants.NumericValidation.TransientEntityId)
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Gets hash code based on Id and Type.
    /// Hash code is stable for persisted entities.
    /// </summary>
    public override int GetHashCode()
    {
        if (Id == DomainConstants.NumericValidation.TransientEntityId)
        {
            return base.GetHashCode();
        }

        return HashCode.Combine(Id, GetType());
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }
}

