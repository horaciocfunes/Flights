namespace SkyRoute.Tests.Validation;

using SkyRoute.Application.Models;
using SkyRoute.Application.Validators;
using Xunit;

public class BookingRequestValidatorTests
{
    private readonly BookingRequestValidator _sut = new();

    private static BookingRequest InternationalRequest(string documentNumber) => new()
    {
        FlightId       = "flight-123",
        IsInternational = true,
        Passengers     =
        [
            new() { FullName = "Jane Doe", Email = "jane@example.com", DocumentNumber = documentNumber }
        ]
    };

    private static BookingRequest DomesticRequest(string documentNumber) => new()
    {
        FlightId       = "flight-456",
        IsInternational = false,
        Passengers     =
        [
            new() { FullName = "John Doe", Email = "john@example.com", DocumentNumber = documentNumber }
        ]
    };

    // ── International: Passport (A-Z 0-9, 6–12 chars) ──────────────────────────

    [Theory]
    [InlineData("AB123456")]       // 8 chars — typical passport
    [InlineData("ABCDEF")]         // 6 chars — minimum
    [InlineData("A1B2C3D4E5F6")]   // 12 chars — maximum
    [InlineData("123456")]         // numeric-only passports are valid
    public void Validate_InternationalRoute_AcceptsValidPassportFormats(string document)
    {
        var result = _sut.Validate(InternationalRequest(document));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("ab123456")]       // lowercase — not allowed
    [InlineData("AB12")]           // too short (< 6)
    [InlineData("AB1234567890X")]  // too long (> 12)
    [InlineData("AB 12345")]       // contains space
    public void Validate_InternationalRoute_RejectsInvalidPassportFormats(string document)
    {
        var result = _sut.Validate(InternationalRequest(document));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("DocumentNumber"));
    }

    // ── Domestic: National ID (digits only, 7–10) ───────────────────────────────

    [Theory]
    [InlineData("1234567")]        // 7 digits — minimum
    [InlineData("12345678")]       // 8 digits
    [InlineData("1234567890")]     // 10 digits — maximum
    public void Validate_DomesticRoute_AcceptsValidNationalIdFormats(string document)
    {
        var result = _sut.Validate(DomesticRequest(document));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("AB123456")]       // alphanumeric — passport format is wrong here
    [InlineData("123456")]         // too short (< 7)
    [InlineData("12345678901")]    // too long (> 10)
    [InlineData("1234 567")]       // contains space
    public void Validate_DomesticRoute_RejectsInvalidNationalIdFormats(string document)
    {
        var result = _sut.Validate(DomesticRequest(document));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("DocumentNumber"));
    }

    // ── General validation ──────────────────────────────────────────────────────

    [Fact]
    public void Validate_WhenPassengersListIsEmpty_ReturnsError()
    {
        var request = new BookingRequest { FlightId = "flight-123", Passengers = [] };

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Passengers");
    }

    [Fact]
    public void Validate_WhenFlightIdIsEmpty_ReturnsError()
    {
        var request = new BookingRequest { FlightId = string.Empty };

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FlightId");
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ReturnsError()
    {
        var request = DomesticRequest("12345678") with
        {
            Passengers = [new() { FullName = "John", Email = "not-an-email", DocumentNumber = "12345678" }]
        };

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Email"));
    }

    [Fact]
    public void Validate_MultiplePassengers_ValidatesEachDocumentIndependently()
    {
        var request = new BookingRequest
        {
            FlightId       = "flight-123",
            IsInternational = true,
            Passengers     =
            [
                new() { FullName = "Jane Doe",  Email = "jane@example.com", DocumentNumber = "AB123456" }, // valid
                new() { FullName = "Bob Smith",  Email = "bob@example.com",  DocumentNumber = "AB12" }      // too short — invalid passport
            ]
        };

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors.Where(e => e.PropertyName.Contains("DocumentNumber")));
    }
}
