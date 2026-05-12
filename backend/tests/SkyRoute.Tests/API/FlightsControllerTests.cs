namespace SkyRoute.Tests.API;

using Microsoft.AspNetCore.Mvc;
using Moq;
using SkyRoute.API.Controllers;
using SkyRoute.API.DTOs;
using SkyRoute.Application.Exceptions;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Models;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;
using Xunit;

public class FlightsControllerTests
{
    private readonly Mock<IFlightSearchService> _searchServiceMock;
    private readonly FlightsController _sut;

    private static readonly Airport JFK = new("JFK", "JFK Airport", "New York",    "United States", "US");
    private static readonly Airport LAX = new("LAX", "LAX Airport", "Los Angeles", "United States", "US");

    public FlightsControllerTests()
    {
        _searchServiceMock = new Mock<IFlightSearchService>();
        _sut               = new FlightsController(_searchServiceMock.Object);
    }

    private static FlightResult MakeResult() => new()
    {
        FlightId          = "f1",
        FlightNumber      = "GA101",
        ProviderName      = "GlobalAir",
        Origin            = JFK,
        Destination       = LAX,
        DepartureTime     = DateTime.UtcNow.AddDays(1),
        ArrivalTime       = DateTime.UtcNow.AddDays(1).AddHours(5),
        Duration          = TimeSpan.FromHours(5),
        CabinClass        = CabinClass.Economy,
        PricePerPassenger = 115m,
        TotalPrice        = 230m,
        PassengerCount    = 2,
        IsInternational   = false
    };

    // ── Happy path ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ValidCriteria_Returns200WithResults()
    {
        _searchServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<FlightSearchCriteria>()))
            .ReturnsAsync(new[] { MakeResult() });

        var result = await _sut.Search(new FlightSearchRequestDto
        {
            OriginCode      = "JFK",
            DestinationCode = "LAX",
            DepartureDate   = DateTime.UtcNow.AddDays(1),
            PassengerCount  = 2,
            CabinClass      = CabinClass.Economy
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    // ── Criteria mapping ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_MapsAllQueryParametersToServiceCriteria()
    {
        FlightSearchCriteria? captured = null;
        _searchServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<FlightSearchCriteria>()))
            .Callback<FlightSearchCriteria>(c => captured = c)
            .ReturnsAsync(Array.Empty<FlightResult>());

        var departure = DateTime.UtcNow.AddDays(3).Date;
        await _sut.Search(new FlightSearchRequestDto
        {
            OriginCode      = "JFK",
            DestinationCode = "LAX",
            DepartureDate   = departure,
            PassengerCount  = 3,
            CabinClass      = CabinClass.Business
        });

        Assert.NotNull(captured);
        Assert.Equal("JFK",               captured!.OriginCode);
        Assert.Equal("LAX",               captured.DestinationCode);
        Assert.Equal(departure,           captured.DepartureDate);
        Assert.Equal(3,                   captured.PassengerCount);
        Assert.Equal(CabinClass.Business, captured.CabinClass);
    }

    // ── Invalid airport propagation ───────────────────────────────────────────────

    [Fact]
    public async Task Search_WhenServiceThrowsInvalidSearchCriteria_ExceptionPropagates()
    {
        _searchServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<FlightSearchCriteria>()))
            .ThrowsAsync(new InvalidSearchCriteriaException("originCode", "Airport 'XXX' not found."));

        await Assert.ThrowsAsync<InvalidSearchCriteriaException>(() =>
            _sut.Search(new FlightSearchRequestDto
            {
                OriginCode      = "XXX",
                DestinationCode = "LAX",
                DepartureDate   = DateTime.UtcNow.AddDays(1)
            }));
    }
}
