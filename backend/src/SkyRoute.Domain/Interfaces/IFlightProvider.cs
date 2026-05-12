namespace SkyRoute.Domain.Interfaces;

using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Models;

public interface IFlightProvider
{
    string ProviderName { get; }
    IPricingStrategy PricingStrategy { get; }
    IEnumerable<Flight> GetFlights(FlightSearchCriteria criteria);
}
