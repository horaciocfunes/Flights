namespace SkyRoute.Tests.Services;

using Microsoft.Extensions.Caching.Memory;
using Moq;
using SkyRoute.Application.Exceptions;
using SkyRoute.Application.Models;
using SkyRoute.Application.Services;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;
using Xunit;

public class BookingServiceTests : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly Mock<IBookingRepository> _repositoryMock;
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _cache          = new MemoryCache(new MemoryCacheOptions());
        _repositoryMock = new Mock<IBookingRepository>();
        _sut            = new BookingService(_cache, _repositoryMock.Object, new BookingRequestValidator());
    }

    public void Dispose() => _cache.Dispose();

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static FlightResult MakeCachedFlight(bool isInternational = false, int passengerCount = 1) => new()
    {
        FlightId          = "flight-abc",
        FlightNumber      = "GA101",
        ProviderName      = "GlobalAir",
        Origin            = new Airport("JFK", "JFK Airport", "New York",  "United States", "US"),
        Destination       = isInternational
            ? new Airport("LHR", "Heathrow",    "London",      "United Kingdom", "GB")
            : new Airport("LAX", "LAX Airport", "Los Angeles", "United States",  "US"),
        DepartureTime     = DateTime.UtcNow.AddDays(1),
        ArrivalTime       = DateTime.UtcNow.AddDays(1).AddHours(5),
        Duration          = TimeSpan.FromHours(5),
        CabinClass        = CabinClass.Economy,
        PricePerPassenger = 115.00m,
        TotalPrice        = 115.00m * passengerCount,
        PassengerCount    = passengerCount,
        IsInternational   = isInternational
    };

    private static BookingRequest DomesticRequest => new()
    {
        FlightId   = "flight-abc",
        Passengers = [new() { FullName = "John Doe", Email = "john@example.com", DocumentNumber = "12345678" }]
    };

    private static BookingRequest InternationalRequest => new()
    {
        FlightId   = "flight-abc",
        Passengers = [new() { FullName = "Jane Doe", Email = "jane@example.com", DocumentNumber = "AB123456" }]
    };

    private void SetupRepository() =>
        _repositoryMock.Setup(r => r.SaveAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

    // ── Cache lookup ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task BookAsync_WhenFlightNotInCache_ThrowsFlightNotFoundException()
    {
        await Assert.ThrowsAsync<FlightNotFoundException>(() => _sut.BookAsync(DomesticRequest));
    }

    // ── Happy paths ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task BookAsync_DomesticFlightWithValidNationalId_ReturnsBookingReference()
    {
        var flight = MakeCachedFlight(isInternational: false); // PricePerPassenger = 115m, 1 passenger
        _cache.Set(flight.FlightId, flight);
        SetupRepository();

        var confirmation = await _sut.BookAsync(DomesticRequest);

        Assert.StartsWith("SKY-", confirmation.BookingReference);
        Assert.Equal(115m, confirmation.TotalPrice);
        Assert.Equal(1,    confirmation.PassengerCount);
    }

    [Fact]
    public async Task BookAsync_InternationalFlightWithValidPassport_ReturnsBookingReference()
    {
        var flight = MakeCachedFlight(isInternational: true); // PricePerPassenger = 115m, 1 passenger
        _cache.Set(flight.FlightId, flight);
        SetupRepository();

        var confirmation = await _sut.BookAsync(InternationalRequest);

        Assert.StartsWith("SKY-", confirmation.BookingReference);
        Assert.Equal(115m, confirmation.TotalPrice);
        Assert.Equal(1,    confirmation.PassengerCount);
    }

    [Fact]
    public async Task BookAsync_TotalPriceEqualsPerPassengerTimesPassengerCount()
    {
        var flight = MakeCachedFlight(isInternational: false, passengerCount: 2);
        _cache.Set(flight.FlightId, flight);
        SetupRepository();

        var request = DomesticRequest with
        {
            Passengers =
            [
                new() { FullName = "P1", Email = "p1@example.com", DocumentNumber = "12345678" },
                new() { FullName = "P2", Email = "p2@example.com", DocumentNumber = "87654321" }
            ]
        };

        var confirmation = await _sut.BookAsync(request);

        Assert.Equal(flight.PricePerPassenger * 2, confirmation.TotalPrice);
        Assert.Equal(2, confirmation.PassengerCount);
    }

    // ── Validation failures ───────────────────────────────────────────────────────

    [Fact]
    public async Task BookAsync_DomesticFlightWithPassportFormat_ThrowsBookingValidationException()
    {
        var flight = MakeCachedFlight(isInternational: false);
        _cache.Set(flight.FlightId, flight);

        var request = DomesticRequest with
        {
            Passengers = [new() { FullName = "John", Email = "john@example.com", DocumentNumber = "AB123456" }]
        };

        await Assert.ThrowsAsync<BookingValidationException>(() => _sut.BookAsync(request));
    }

    [Fact]
    public async Task BookAsync_InternationalFlightWithNationalIdFormat_ThrowsBookingValidationException()
    {
        var flight = MakeCachedFlight(isInternational: true);
        _cache.Set(flight.FlightId, flight);

        var request = InternationalRequest with
        {
            Passengers = [new() { FullName = "Jane", Email = "jane@example.com", DocumentNumber = "ab123456" }]
        };

        await Assert.ThrowsAsync<BookingValidationException>(() => _sut.BookAsync(request));
    }

    // ── Side-effect verification ─────────────────────────────────────────────────

    [Fact]
    public async Task BookAsync_ValidBooking_CallsSaveAsyncExactlyOnce()
    {
        var flight = MakeCachedFlight(isInternational: false);
        _cache.Set(flight.FlightId, flight);
        SetupRepository();

        await _sut.BookAsync(DomesticRequest);

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task BookAsync_WhenValidationFails_NeverCallsSaveAsync()
    {
        var flight = MakeCachedFlight(isInternational: false);
        _cache.Set(flight.FlightId, flight);

        var invalidRequest = DomesticRequest with
        {
            Passengers = [new() { FullName = "", Email = "bad", DocumentNumber = "AB123456" }]
        };

        await Assert.ThrowsAsync<BookingValidationException>(() => _sut.BookAsync(invalidRequest));

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Booking>()), Times.Never);
    }

    // ── Passenger count mismatch ──────────────────────────────────────────────────

    [Fact]
    public async Task BookAsync_WhenPassengerCountDoesNotMatchFlight_ShouldThrowValidationException()
    {
        var flight = MakeCachedFlight(passengerCount: 3);
        _cache.Set(flight.FlightId, flight);

        await Assert.ThrowsAsync<BookingValidationException>(() => _sut.BookAsync(DomesticRequest));

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Booking>()), Times.Never);
    }

    // ── IsInternational enrichment contract ───────────────────────────────────────

    [Fact]
    public async Task BookAsync_EnrichesIsInternational_FromCachedFlight_NotFromRequest()
    {
        var flight = MakeCachedFlight(isInternational: true);
        _cache.Set(flight.FlightId, flight);
        SetupRepository();

        // Request carries IsInternational = false (default) but cached flight is international.
        // Enrichment must overwrite it; "AB123456" is valid passport but not valid national ID.
        // If enrichment were removed this test would throw BookingValidationException.
        var request = new BookingRequest
        {
            FlightId   = "flight-abc",
            Passengers = [new() { FullName = "Jane", Email = "jane@example.com", DocumentNumber = "AB123456" }]
        };

        var confirmation = await _sut.BookAsync(request);

        Assert.False(string.IsNullOrEmpty(confirmation.BookingReference));
    }
}
