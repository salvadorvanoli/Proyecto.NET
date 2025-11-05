namespace Domain.Constants;

/// <summary>
/// Contains all domain-level constants to avoid magic numbers and strings.
/// Centralizes validation rules, length constraints, and other domain constants.
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// String length validation constants for various domain entities.
    /// </summary>
    public static class StringLengths
    {
        // General Names
        public const int NameMinLength = 2;
        public const int NameMaxLength = 200;

        // User-related
        public const int EmailMaxLength = 254; // RFC 5321 standard
        public const int PasswordHashMaxLength = 512;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;

        // Location/Address
        public const int StreetMaxLength = 200;
        public const int NumberMaxLength = 20;
        public const int CityMaxLength = 100;
        public const int CountryMaxLength = 100;

        // Content/Descriptions
        public const int TitleMaxLength = 300;
        public const int DescriptionMaxLength = 1000;
        public const int MessageMaxLength = 500;

        // Other entities
        public const int RoleNameMaxLength = 100;
        public const int SpaceTypeNameMaxLength = 200;
        public const int ControlPointNameMaxLength = 200;
    }

    /// <summary>
    /// Date and time validation constants.
    /// </summary>
    public static class DateTimeValidation
    {
        // Date range limits
        public const int MinYear = 1900;
        public const int MaxYear = 2100;
        public const int MinMonth = 1;
        public const int MaxMonth = 12;
        public const int MinDay = 1;
        public const int MaxDay = 31;

        // Age validation
        public const int MinAge = 0;
        public const int MaxAge = 150;
    }

    /// <summary>
    /// Numeric validation constants for IDs, quotas, and amounts.
    /// </summary>
    public static class NumericValidation
    {
        // Identity validation
        public const int MinId = 1;
        public const int TransientEntityId = 0; // Entity not yet persisted

        // Quantity validation
        public const int MinQuota = 0;
        public const int MinQuantity = 0;
        public const int MinAmount = 0;
    }

    /// <summary>
    /// Standard error messages for validation failures.
    /// Use string.Format or string interpolation with these templates.
    /// </summary>
    public static class ErrorMessages
    {
        // Null/Empty validation
        public const string CannotBeNullOrEmpty = "{0} cannot be null or empty.";

        // Length validation
        public const string MinLengthRequired = "{0} must be at least {1} characters long.";
        public const string MaxLengthExceeded = "{0} cannot exceed {1} characters.";
        public const string LengthMustBeBetween = "{0} must be between {1} and {2} characters.";

        // Numeric validation
        public const string MustBeGreaterThanZero = "{0} must be greater than zero.";
        public const string MustBeGreaterThanOrEqualTo = "{0} must be greater than or equal to {1}.";
        public const string MustBeLessThanOrEqualTo = "{0} must be less than or equal to {1}.";
        public const string MustBeBetween = "{0} must be between {1} and {2}.";

        // Date/Time validation
        public const string DateCannotBeBefore = "{0} cannot be before {1}.";
        public const string DateCannotBeAfter = "{0} cannot be after {1}.";
        public const string DateCannotBeInTheFuture = "{0} cannot be in the future.";
        public const string InvalidDateFormat = "Invalid date format for {0}.";
        public const string InvalidTimeFormat = "Invalid {0} format: {1}";
        public const string TimeCannotBeNullOrEmpty = "{0} cannot be null or empty.";

        // Entity validation
        public const string InvalidEmailFormat = "Invalid email format.";
        public const string InvalidCharacters = "{0} contains invalid characters.";
        public const string MustBelongToSameTenant = "{0} must belong to the same tenant.";
    }

    /// <summary>
    /// Regular expression patterns for validation.
    /// </summary>
    public static class RegexPatterns
    {
        // Email validation (simplified RFC 5322)
        public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // Other patterns can be added here as needed
        // public const string PhoneNumber = @"^\+?[1-9]\d{1,14}$";
    }

    /// <summary>
    /// System roles that cannot be edited or deleted.
    /// </summary>
    public static class SystemRoles
    {
        public const string AdministradorBackoffice = "AdministradorBackoffice";

        /// <summary>
        /// Check if a role name is a protected system role.
        /// </summary>
        public static bool IsProtectedRole(string roleName)
        {
            return roleName == AdministradorBackoffice;
        }
    }
}
