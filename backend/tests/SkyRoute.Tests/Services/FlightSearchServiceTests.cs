namespace SkyRoute.Tests.Services;

using Microsoft.Extensions.Caching.Memory;
using Moq;
using SkyRoute.Application.Exceptions;
using SkyRoute.Application.Models;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;
using SkyRoute.Domain.Models;
using Xunit;

public class FlightSearchServiceTests : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly Mock<IFlightProvider> _provider1;
    private readonly Mock<IFlightProvider> _provider2;
    private readonly Mock<IPricingStrategy> _strategy1;
    private readonly Mock<IPricingStrategy> _strategy2;
    private readonly Mock<IAirportRepository> _airportRepo;
    private readonly FlightSearchService _sut;

    private static readonly Airport JFK = new("JFK", "JFK Airport", "New York",     "United States",  "US");
    private static readonly Airport LAX = new("LAX", "LAX Airport", "Los Angeles",  "United States",  "US");
    private static readonly Airport LHR = new("LHR", "Heathrow",    "London",       "United Kingdom", "GB");

    private static readonly FlightSearchCriteria DomesticCriteria = new(
        "JFK", "LAX", DateTime.UtcNow.AddDays(1), 2, CabinClass.Economy);

    private static readonly FlightSearchCriteria InternationalCriteria = new(
        "JFK", "LHR", DateTime.UtcNow.AddDays(1), 1, CabinClass.Economy);

    public FlightSearchServiceTests()
    {
        _cache       = new MemoryCache(new MemoryCacheOptions());
        _strategy1   = new Mock<IPricingStrategy>();
        _strategy2   = new Mock<IPricingStrategy>();
        _provider1   = new Mock<IFlightProvider>();
        _provider2   = new Mock<IFlightProvider>();
        _airportRepo = new Mock<IAirportRepository>();

        _provider1.Setup(p => p.PricingStrategy).Returns(_strategy1.Object);
        _provider2.Setup(p => p.PricingStrategy).Returns(_strategy2.Object);

        _airportRepo.Setup(r => r.GetByCode("JFK")).Returns(JFK);
        _airportRepo.Setup(r => r.GetByCode("LAX")).Returns(LAX);
        _airportRepo.Setup(r => r.GetByCode("LHR")).Returns(LHR);

        _sut = new FlightSearchService(
            new[] { _provider1.Object, _provider2.Object },
            _cache,
            _airportRepo.Object);
    }

    public void Dispose() => _cache.Dispose();

    private static Flight MakeFlight(string id, decimal baseFare, Airport origin, Airport destination) => new()
    {
        Id            = id,
        FlightNumber  = $"FL{id}",
        ProviderName  = "Test",
        Origin        = origin,
        Destination   = destination,
        DepartureTime = DateTime.UtcNow.AddDays(1),
        ArrivalTime   = DateTime.UtcNow.AddDays(1).AddHours(5),
        CabinClass    = CabinClass.Economy,
        BaseFare      = baseFare
    };

    // ── Aggregation ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_AggregatesFlightsFromBothProviders_ReturnsCombinedResults()
    {
        _provider1.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f1", 100m, JFK, LAX) });
        _provider2.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f2", 200m, JFK, LAX) });
        _strategy1.Setup(s => s.Calculate(It.IsAny<decimal>())).Returns<decimal>(b => b);
        _strategy2.Setup(s => s.Calculate(It.IsAny<decimal>())).Returns<decimal>(b => b);

        var results = (await _sut.SearchAsync(DomesticCriteria)).ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.FlightId == "f1");
        Assert.Contains(results, r => r.FlightId == "f2");
    }

    // ── Pricing strategy isolation ────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_AppliesEachProvidersPricingStrategy_IndependentlyPerProvider()
    {
        _provider1.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f1", 100m, JFK, LAX) });
        _provider2.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f2", 100m, JFK, LAX) });
        _strategy1.Setup(s => s.Calculate(100m)).Returns(115m);
        _strategy2.Setup(s => s.Calculate(100m)).Returns(90m);

        var results = (await _sut.SearchAsync(DomesticCriteria)).ToList();

        Assert.Equal(115m, results.Single(r => r.FlightId == "f1").PricePerPassenger);
        Assert.Equal(90m,  results.Single(r => r.FlightId == "f2").PricePerPassenger);
    }

    // ── Cache population ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_CachesEachResultByFlightId()
    {
        _provider1.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f1", 100m, JFK, LAX) });
        _provider2.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(Array.Empty<Flight>());
        _strategy1.Setup(s => s.Calculate(100m)).Returns(100m);

        await _sut.SearchAsync(DomesticCriteria); // DomesticCriteria has PassengerCount = 2

        Assert.True(_cache.TryGetValue("f1", out FlightResult? cached));
        Assert.NotNull(cached);
        Assert.Equal(100m,  cached!.PricePerPassenger);
        Assert.Equal(200m,  cached.TotalPrice);          // 100 × 2 passengers
        Assert.Equal(2,     cached.PassengerCount);
        Assert.False(cached.IsInternational);
    }

    // ── DocumentLabel ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_DomesticFlight_DocumentLabelIsNationalId()
    {
        _provider1.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f1", 100m, JFK, LAX) });
        _provider2.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(Array.Empty<Flight>());
        _strategy1.Setup(s => s.Calculate(It.IsAny<decimal>())).Returns(100m);

        var results = (await _sut.SearchAsync(DomesticCriteria)).ToList();

        Assert.Equal("National ID", results.Single().DocumentLabel);
    }

    [Fact]
    public async Task SearchAsync_InternationalFlight_DocumentLabelIsPassportNumber()
    {
        _provider1.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f1", 100m, JFK, LHR) });
        _provider2.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(Array.Empty<Flight>());
        _strategy1.Setup(s => s.Calculate(It.IsAny<decimal>())).Returns(500m);

        var results = (await _sut.SearchAsync(InternationalCriteria)).ToList();

        Assert.Equal("Passport Number", results.Single().DocumentLabel);
    }

    // ── Provider fault tolerance ──────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_WhenOneProviderThrows_ReturnsResultsFromRemainingProvider()
    {
        _provider1.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Throws(new Exception("Provider 1 is down"));
        _provider2.Setup(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()))
                  .Returns(new[] { MakeFlight("f2", 150m, JFK, LAX) });
        _strategy2.Setup(s => s.Calculate(It.IsAny<decimal>())).Returns(150m);

        var results = (await _sut.SearchAsync(DomesticCriteria)).ToList();

        Assert.Single(results);
        Assert.Equal("f2", results[0].FlightId);
    }

    // ── Airport validation ────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_WhenOriginCodeIsUnknown_ThrowsInvalidSearchCriteriaExceptionAndDoesNotCallProviders()
    {
        var criteria = new FlightSearchCriteria("XXX", "LAX", DateTime.UtcNow.AddDays(1), 1, CabinClass.Economy);

        await Assert.ThrowsAsync<InvalidSearchCriteriaException>(() => _sut.SearchAsync(criteria));

        _provider1.Verify(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()), Times.Never);
        _provider2.Verify(p => p.GetFlights(It.IsAny<FlightSearchCriteria>()), Times.Never);
    }
}
