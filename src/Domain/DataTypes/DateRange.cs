using Domain.Constants;

namespace Domain.DataTypes;

/// <summary>
/// Value object that represents a date range with start and end dates.
/// </summary>
public readonly record struct DateRange
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    public DateRange(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("La fecha de inicio debe ser anterior o igual a la fecha de fin.", nameof(startDate));

        if (startDate < new DateOnly(DomainConstants.DateTimeValidation.MinYear, DomainConstants.DateTimeValidation.MinMonth, DomainConstants.DateTimeValidation.MinDay))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.DateCannotBeBefore, "Fecha de inicio", $"{DomainConstants.DateTimeValidation.MinYear}-01-01"),
                nameof(startDate));

        if (endDate > new DateOnly(DomainConstants.DateTimeValidation.MaxYear, DomainConstants.DateTimeValidation.MaxMonth, DomainConstants.DateTimeValidation.MaxDay))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.DateCannotBeAfter, "Fecha de fin", $"{DomainConstants.DateTimeValidation.MaxYear}-12-31"),
                nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
    }

    public DateRange(DateTime startDate, DateTime endDate)
        : this(DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate))
    {
    }

    public int DurationInDays => EndDate.DayNumber - StartDate.DayNumber + 1;

    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;

    public bool Contains(DateTime dateTime) => Contains(DateOnly.FromDateTime(dateTime));

    public bool IsActive => Contains(DateOnly.FromDateTime(DateTime.Today));

    public bool Overlaps(DateRange other) =>
        StartDate <= other.EndDate && EndDate >= other.StartDate;

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}

