namespace SkyRoute.Infrastructure.Providers;

using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;

public class BudgetWingsProvider : IFlightProvider
{
    private readonly IAirportRepository _airports;

    public BudgetWingsProvider(IAirportRepository airports, BudgetWingsPricingStrategy pricingStrategy)
    {
        _airports = airports;
        PricingStrategy = pricingStrategy;
    }

    public string ProviderName => "BudgetWings";
    public IPricingStrategy PricingStrategy { get; }

    public IEnumerable<Flight> GetFlights(FlightSearchCriteria criteria)
    {
        var origin = _airports.GetByCode(criteria.OriginCode);
        var destination = _airports.GetByCode(criteria.DestinationCode);

        if (origin is null || destination is null)
            return [];

        // Offset of 42 from GlobalAir seed so both providers return different flights.
        var seed = criteria.DepartureDate.DayOfYear
            + Math.Abs(criteria.OriginCode.GetHashCode())
            + Math.Abs(criteria.DestinationCode.GetHashCode())
            + 42;

        var rng = new Random(seed);
        var count = rng.Next(5, 11);

        return Enumerable.Range(0, count)
            .Select(i => CreateFlight(i, origin, destination, criteria, rng));
    }

    private Flight CreateFlight(
        int index, Airport origin, Airport destination,
        FlightSearchCriteria criteria, Random rng)
    {
        var departure = criteria.DepartureDate.Date
            .AddMinutes(30 + index * 95 + rng.Next(0, 30));
        var durationMin = BaseDurationMinutes(origin, destination) + rng.Next(-15, 31);

        return new Flight
        {
            Id = $"{ProviderName}-{origin.Code}-{destination.Code}-{departure:yyyyMMddHHmm}",
            FlightNumber = $"BW{200 + index + rng.Next(0, 50)}",
            ProviderName = ProviderName,
            Origin = origin,
            Destination = destination,
            DepartureTime = departure,
            ArrivalTime = departure.AddMinutes(durationMin),
            CabinClass = criteria.CabinClass,
            BaseFare = BaseFare(origin, destination, criteria.CabinClass, rng)
        };
    }

    private static int BaseDurationMinutes(Airport origin, Airport destination) =>
        origin.CountryCode == destination.CountryCode ? 195
        : destination.CountryCode switch
        {
            "GB" => 435,
            "ES" => 495,
            "AR" => 675,
            _    => 375
        };

    private static decimal BaseFare(Airport origin, Airport destination, CabinClass cabin, Random rng)
    {
        var route = origin.CountryCode == destination.CountryCode ? 120m : 480m;
        var multiplier = cabin switch
        {
            CabinClass.Business => 2.2m,
            CabinClass.First    => 3.5m,
            _                   => 1.0m
        };
        return Math.Round((route + rng.Next(-15, 41)) * multiplier, 2);
    }
}
