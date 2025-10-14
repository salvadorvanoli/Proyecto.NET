using Domain.Constants;

namespace Domain.DataTypes;

/// <summary>
/// Value object that represents a time range with start and end times.
/// Supports ranges that cross midnight (e.g., 22:00 to 02:00).
/// </summary>
public readonly record struct TimeRange
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }

    public TimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public TimeRange(string startTime, string endTime)
    {
        if (string.IsNullOrWhiteSpace(startTime))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.TimeCannotBeNullOrEmpty, "Start time"),
                nameof(startTime));

        if (string.IsNullOrWhiteSpace(endTime))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.TimeCannotBeNullOrEmpty, "End time"),
                nameof(endTime));

        TimeOnly parsedStart;
        TimeOnly parsedEnd;

        if (!TimeOnly.TryParse(startTime, out parsedStart))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.InvalidTimeFormat, "start time", startTime),
                nameof(startTime));

        if (!TimeOnly.TryParse(endTime, out parsedEnd))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.InvalidTimeFormat, "end time", endTime),
                nameof(endTime));

        StartTime = parsedStart;
        EndTime = parsedEnd;
    }

    public TimeSpan Duration => EndTime < StartTime
        ? TimeSpan.FromHours(24) - (StartTime - EndTime).Duration()
        : EndTime - StartTime;

    public bool Contains(TimeOnly time)
    {
        if (EndTime < StartTime)
        {
            return time >= StartTime || time <= EndTime;
        }
        else
        {
            return time >= StartTime && time <= EndTime;
        }
    }

    public bool Contains(DateTime dateTime) => Contains(TimeOnly.FromDateTime(dateTime));

    public bool Overlaps(TimeRange other)
    {
        if (EndTime < StartTime || other.EndTime < StartTime)
        {
            return Contains(other.StartTime) || Contains(other.EndTime) ||
                   other.Contains(StartTime) || other.Contains(EndTime);
        }
        else
        {
            return StartTime < other.EndTime && EndTime > other.StartTime;
        }
    }

    public override string ToString() => EndTime < StartTime
        ? $"{StartTime:HH:mm} - {EndTime:HH:mm} (crosses midnight)"
        : $"{StartTime:HH:mm} - {EndTime:HH:mm}";
}

