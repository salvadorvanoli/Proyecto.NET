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
        public const int EmailMinLength = 5;
        public const int EmailMaxLength = 256; // RFC 5321 standard (updated for validation)
        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 100;
        public const int PasswordHashMaxLength = 512;
        public const int FirstNameMinLength = 2;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMinLength = 2;
        public const int LastNameMaxLength = 100;

        // Location/Address
        public const int StreetMaxLength = 200;
        public const int NumberMaxLength = 20;
        public const int CityMaxLength = 100;
        public const int CountryMaxLength = 100;

        // Content/Descriptions
        public const int TitleMinLength = 5;
        public const int TitleMaxLength = 200;
        public const int DescriptionMinLength = 10;
        public const int DescriptionMaxLength = 500;
        public const int ContentMinLength = 10;
        public const int ContentMaxLength = 5000;
        public const int MessageMaxLength = 500;
        public const int UrlMaxLength = 500;

        // Entity-specific names
        public const int RoleNameMinLength = 2;
        public const int RoleNameMaxLength = 100;
        public const int SpaceTypeNameMinLength = 2;
        public const int SpaceTypeNameMaxLength = 200;
        public const int SpaceNameMinLength = 2;
        public const int SpaceNameMaxLength = 200;
        public const int ControlPointNameMinLength = 2;
        public const int ControlPointNameMaxLength = 200;
        public const int BenefitTypeNameMinLength = 2;
        public const int BenefitTypeNameMaxLength = 100;
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
        public const int MinQuota = 1;
        public const int MinQuantity = 0;
        public const int MinAmount = 0;
        public const int MinRoleCount = 1;
        public const int MinControlPointCount = 1;
    }

    /// <summary>
    /// Standard error messages for validation failures.
    /// Use string.Format or string interpolation with these templates.
    /// </summary>
    public static class ErrorMessages
    {
        // Null/Empty validation
        public const string CannotBeNullOrEmpty = "{0} no puede ser nulo o vacío.";

        // Length validation
        public const string MinLengthRequired = "{0} debe tener al menos {1} caracteres.";
        public const string MaxLengthExceeded = "{0} no puede exceder {1} caracteres.";
        public const string LengthMustBeBetween = "{0} debe tener entre {1} y {2} caracteres.";

        // Numeric validation
        public const string MustBeGreaterThanZero = "{0} debe ser mayor que cero.";
        public const string MustBeGreaterThanOrEqualTo = "{0} debe ser mayor o igual a {1}.";
        public const string MustBeLessThanOrEqualTo = "{0} debe ser menor o igual a {1}.";
        public const string MustBeBetween = "{0} debe estar entre {1} y {2}.";

        // Date/Time validation
        public const string DateCannotBeBefore = "{0} no puede ser anterior a {1}.";
        public const string DateCannotBeAfter = "{0} no puede ser posterior a {1}.";
        public const string DateCannotBeInTheFuture = "{0} no puede estar en el futuro.";
        public const string InvalidDateFormat = "Formato de fecha inválido para {0}.";
        public const string InvalidTimeFormat = "Formato de {0} inválido: {1}";
        public const string TimeCannotBeNullOrEmpty = "{0} no puede ser nulo o vacío.";

        // Entity validation
        public const string InvalidEmailFormat = "Formato de email inválido.";
        public const string InvalidCharacters = "{0} contiene caracteres inválidos.";
        public const string MustBelongToSameTenant = "{0} debe pertenecer al mismo tenant.";
    }

    /// <summary>
    /// Regular expression patterns for validation.
    /// </summary>
    public static class RegexPatterns
    {
        // Email validation (simplified RFC 5322)
        public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        
        // Time validation (HH:mm format)
        public const string Time24Hour = @"^([01]?[0-9]|2[0-3]):[0-5][0-9]$";

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
