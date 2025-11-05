using Domain.Constants;
using Domain.DataTypes;

namespace Domain.Entities;

/// <summary>
/// Represents a user in the system with personal information and credentials.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Email address of the user (used for authentication).
    /// </summary>
    public string Email { get; protected set; }

    /// <summary>
    /// Password hash for authentication.
    /// </summary>
    public string PasswordHash { get; protected set; }

    /// <summary>
    /// Personal data of the user.
    /// </summary>
    public PersonalData PersonalData { get; protected set; }

    /// <summary>
    /// Foreign key to the user's credential (can be null).
    /// </summary>
    public int? CredentialId { get; protected set; }

    // Navigation properties
    public virtual Credential? Credential { get; protected set; }
    public virtual ICollection<Role> Roles { get; protected set; } = new List<Role>();
    public virtual ICollection<Notification> Notifications { get; protected set; } = new List<Notification>();
    public virtual ICollection<AccessEvent> AccessEvents { get; protected set; } = new List<AccessEvent>();
    public virtual ICollection<Usage> Usages { get; protected set; } = new List<Usage>();

    protected User() : base()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(int tenantId, string email, string passwordHash, PersonalData personalData) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Email"),
                nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Password hash"),
                nameof(passwordHash));

        if (!IsValidEmail(email))
            throw new ArgumentException(DomainConstants.ErrorMessages.InvalidEmailFormat, nameof(email));

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        PersonalData = personalData;
    }

    /// <summary>
    /// Updates the user's email address.
    /// </summary>
    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Email"),
                nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException(DomainConstants.ErrorMessages.InvalidEmailFormat, nameof(email));

        Email = email.Trim().ToLowerInvariant();
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    public void UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Password hash"),
                nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the user's personal data.
    /// </summary>
    public void UpdatePersonalData(PersonalData personalData)
    {
        PersonalData = personalData;
        UpdateTimestamp();
    }

    /// <summary>
    /// Assigns a credential to the user.
    /// </summary>
    public void AssignCredential(int credentialId)
    {
        if (credentialId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Credential ID"),
                nameof(credentialId));

        CredentialId = credentialId;
        UpdateTimestamp();
    }

    /// <summary>
    /// Removes the credential from the user.
    /// </summary>
    public void RemoveCredential()
    {
        CredentialId = null;
        UpdateTimestamp();
    }

    /// <summary>
    /// Assigns a role to the user.
    /// </summary>
    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!Roles.Contains(role))
        {
            Roles.Add(role);
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (Roles.Contains(role))
        {
            Roles.Remove(role);
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public bool HasRole(string roleName)
    {
        return Roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the user has an active credential.
    /// </summary>
    public bool HasActiveCredential => Credential?.IsActive == true;

    /// <summary>
    /// Gets the user's full name from personal data.
    /// </summary>
    public string FullName => PersonalData.FullName;

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
