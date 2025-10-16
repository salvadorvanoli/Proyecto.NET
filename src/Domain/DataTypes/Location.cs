using Domain.Constants;

namespace Domain.DataTypes;

/// <summary>
/// Value object that represents a geographical location with street address details.
/// </summary>
public readonly record struct Location
{
    public string Street { get; init; }
    public string Number { get; init; }
    public string City { get; init; }
    public string Country { get; init; }

    public Location(string street, string number, string city, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Street"),
                nameof(street));
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Number"),
                nameof(number));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "City"),
                nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Country"),
                nameof(country));

        var trimmedStreet = street.Trim();
        var trimmedNumber = number.Trim();
        var trimmedCity = city.Trim();
        var trimmedCountry = country.Trim();

        if (trimmedStreet.Length > DomainConstants.StringLengths.StreetMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Street", DomainConstants.StringLengths.StreetMaxLength),
                nameof(street));

        if (trimmedNumber.Length > DomainConstants.StringLengths.NumberMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Number", DomainConstants.StringLengths.NumberMaxLength),
                nameof(number));

        if (trimmedCity.Length > DomainConstants.StringLengths.CityMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "City", DomainConstants.StringLengths.CityMaxLength),
                nameof(city));

        if (trimmedCountry.Length > DomainConstants.StringLengths.CountryMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Country", DomainConstants.StringLengths.CountryMaxLength),
                nameof(country));

        Street = trimmedStreet;
        Number = trimmedNumber;
        City = trimmedCity;
        Country = trimmedCountry;
    }

    public static Location Create(string street, string number, string city, string country)
        => new(street, number, city, country);

    public string GetFullAddress()
        => $"{Street} {Number}, {City}, {Country}";

    public override string ToString() => GetFullAddress();
}

