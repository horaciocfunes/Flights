namespace SkyRoute.Application.Services;

using Microsoft.Extensions.Caching.Memory;
using SkyRoute.Application.Exceptions;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Models;
using SkyRoute.Domain.Interfaces;
using SkyRoute.Domain.Models;

public class FlightSearchService : IFlightSearchService
{
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly IMemoryCache _cache;
    private readonly IAirportRepository _airports;

    public FlightSearchService(
        IEnumerable<IFlightProvider> providers,
        IMemoryCache cache,
        IAirportRepository airports)
    {
        _providers = providers;
        _cache = cache;
        _airports = airports;
    }

    public Task<IEnumerable<FlightResult>> SearchAsync(FlightSearchCriteria criteria)
    {
        if (_airports.GetByCode(criteria.OriginCode) is null)
            throw new InvalidSearchCriteriaException("originCode",
                $"Airport '{criteria.OriginCode}' not found.");

        if (_airports.GetByCode(criteria.DestinationCode) is null)
            throw new InvalidSearchCriteriaException("destinationCode",
                $"Airport '{criteria.DestinationCode}' not found.");

        var results = new List<FlightResult>();

        foreach (var provider in _providers)
        {
            List<FlightResult> chunk = [];

            try
            {
                chunk = provider.GetFlights(criteria)
                    .Select(flight =>
                    {
                        var pricePerPassenger = provider.PricingStrategy.Calculate(flight.BaseFare);

                        return new FlightResult
                        {
                            FlightId = flight.Id,
                            FlightNumber = flight.FlightNumber,
                            ProviderName = flight.ProviderName,
                            Origin = flight.Origin,
                            Destination = flight.Destination,
                            DepartureTime = flight.DepartureTime,
                            ArrivalTime = flight.ArrivalTime,
                            Duration = flight.Duration,
                            CabinClass = flight.CabinClass,
                            BaseFare = flight.BaseFare,
                            PricePerPassenger = pricePerPassenger,
                            TotalPrice = pricePerPassenger * criteria.PassengerCount,
                            PassengerCount = criteria.PassengerCount,
                            IsInternational = flight.IsInternational
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Provider {provider.GetType().Name} failed: {ex.Message}");
            }

            results.AddRange(chunk);
        }

        results.Sort((a, b) => a.DepartureTime.CompareTo(b.DepartureTime));

        foreach (var result in results)
            _cache.Set(result.FlightId, result, TimeSpan.FromMinutes(90));

        return Task.FromResult<IEnumerable<FlightResult>>(results);
    }
}
