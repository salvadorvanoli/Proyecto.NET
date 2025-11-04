using Domain.Constants;

namespace Domain.DataTypes;

/// <summary>
/// Value object that represents personal data information.
/// </summary>
public readonly record struct PersonalData
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly BirthDate { get; init; }

    public PersonalData(string firstName, string lastName, DateOnly birthDate)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Apellido"),
                nameof(lastName));

        if (!IsValidName(firstName))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.InvalidCharacters, "Nombre"),
                nameof(firstName));

        if (!IsValidName(lastName))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.InvalidCharacters, "Apellido"),
                nameof(lastName));

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (birthDate > today)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.DateCannotBeInTheFuture, "Fecha de nacimiento"),
                nameof(birthDate));

        var age = CalculateAge(birthDate, today);
        if (age > DomainConstants.DateTimeValidation.MaxAge)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeLessThanOrEqualTo, "Edad", DomainConstants.DateTimeValidation.MaxAge),
                nameof(birthDate));

        if (age < DomainConstants.DateTimeValidation.MinAge)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.DateCannotBeInTheFuture, "Fecha de nacimiento"),
                nameof(birthDate));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        BirthDate = birthDate;
    }

    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Calculates the age correctly handling leap years and birth dates.
    /// </summary>
    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return CalculateAge(BirthDate, today);
        }
    }

    /// <summary>
    /// Validates that a name contains only valid characters.
    /// </summary>
    private static bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // ✅ Permite letras (unicode), espacios, guiones, apóstrofes y tildes
        foreach (char c in name)
        {
            if (!char.IsLetter(c) && c != ' ' && c != '-' && c != '\'' && c != '.')
                return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates age correctly comparing both year, month and day.
    /// </summary>
    private static int CalculateAge(DateOnly birthDate, DateOnly referenceDate)
    {
        int age = referenceDate.Year - birthDate.Year;

        // ✅ Corregido: Comparar mes y día correctamente
        if (referenceDate.Month < birthDate.Month ||
            (referenceDate.Month == birthDate.Month && referenceDate.Day < birthDate.Day))
        {
            age--;
        }

        return age;
    }

    public override string ToString() => FullName;
}

