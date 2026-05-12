namespace SkyRoute.Tests.API;

using Microsoft.AspNetCore.Mvc;
using Moq;
using SkyRoute.API.Controllers;
using SkyRoute.API.DTOs;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Models;
using Xunit;

public class BookingsControllerTests
{
    private readonly Mock<IBookingService> _bookingServiceMock;
    private readonly BookingsController _sut;

    public BookingsControllerTests()
    {
        _bookingServiceMock = new Mock<IBookingService>();
        _sut                = new BookingsController(_bookingServiceMock.Object);
    }

    private static BookingRequestDto ValidDto() => new()
    {
        FlightId   = "flight-abc",
        Passengers = [new() { FullName = "John Doe", Email = "john@example.com", DocumentNumber = "12345678" }]
    };

    // ── Happy path ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Book_ValidRequest_Returns201WithBookingReference()
    {
        _bookingServiceMock
            .Setup(s => s.BookAsync(It.IsAny<BookingRequest>()))
            .ReturnsAsync(new BookingConfirmation
            {
                BookingReference = "SKY-ABC123",
                TotalPrice       = 115m,
                PassengerCount   = 1
            });

        var result = await _sut.Book(ValidDto());

        var created  = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);
        var response = Assert.IsType<BookingResponseDto>(created.Value);
        Assert.Equal("SKY-ABC123", response.BookingReference);
        Assert.Equal(115m,         response.TotalPrice);
        Assert.Equal(1,            response.PassengerCount);
    }

    // ── DTO mapping ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Book_MapsAllDtoFieldsToBookingRequest()
    {
        BookingRequest? captured = null;
        _bookingServiceMock
            .Setup(s => s.BookAsync(It.IsAny<BookingRequest>()))
            .Callback<BookingRequest>(r => captured = r)
            .ReturnsAsync(new BookingConfirmation { BookingReference = "SKY-X", TotalPrice = 0, PassengerCount = 1 });

        await _sut.Book(new BookingRequestDto
        {
            FlightId   = "flight-xyz",
            Passengers = [new() { FullName = "Alice", Email = "alice@example.com", DocumentNumber = "AB123456" }]
        });

        Assert.NotNull(captured);
        Assert.Equal("flight-xyz",        captured!.FlightId);
        Assert.Equal("Alice",             captured.Passengers[0].FullName);
        Assert.Equal("alice@example.com", captured.Passengers[0].Email);
        Assert.Equal("AB123456",          captured.Passengers[0].DocumentNumber);
    }
}
