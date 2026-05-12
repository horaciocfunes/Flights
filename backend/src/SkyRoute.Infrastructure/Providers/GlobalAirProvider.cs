namespace SkyRoute.Infrastructure.Providers;

using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;

public class GlobalAirProvider : IFlightProvider
{
    private readonly IAirportRepository _airports;

    public GlobalAirProvider(IAirportRepository airports, GlobalAirPricingStrategy pricingStrategy)
    {
        _airports = airports;
        PricingStrategy = pricingStrategy;
    }

    public string ProviderName => "GlobalAir";
    public IPricingStrategy PricingStrategy { get; }

    public IEnumerable<Flight> GetFlights(FlightSearchCriteria criteria)
    {
        var origin = _airports.GetByCode(criteria.OriginCode);
        var destination = _airports.GetByCode(criteria.DestinationCode);

        if (origin is null || destination is null)
            return [];

        // Seed is deterministic per (date + route) so the same search always yields the same flights.
        var seed = criteria.DepartureDate.DayOfYear
            + Math.Abs(criteria.OriginCode.GetHashCode())
            + Math.Abs(criteria.DestinationCode.GetHashCode());

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
            .AddMinutes(60 + index * 110 + rng.Next(0, 45));
        var durationMin = BaseDurationMinutes(origin, destination) + rng.Next(-20, 41);

        return new Flight
        {
            Id = $"{ProviderName}-{origin.Code}-{destination.Code}-{departure:yyyyMMddHHmm}",
            FlightNumber = $"GA{100 + index + rng.Next(0, 50)}",
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
        origin.CountryCode == destination.CountryCode ? 180
        : destination.CountryCode switch
        {
            "GB" => 420,
            "ES" => 480,
            "AR" => 660,
            _    => 360
        };

    private static decimal BaseFare(Airport origin, Airport destination, CabinClass cabin, Random rng)
    {
        var route = origin.CountryCode == destination.CountryCode ? 150m : 550m;
        var multiplier = cabin switch
        {
            CabinClass.Business => 2.5m,
            CabinClass.First    => 4.0m,
            _                   => 1.0m
        };
        return Math.Round((route + rng.Next(-20, 51)) * multiplier, 2);
    }
}
