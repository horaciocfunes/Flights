namespace SkyRoute.Domain.Entities;

using SkyRoute.Domain.Enums;

public class Flight
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string FlightNumber { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public Airport Origin { get; init; } = default!;
    public Airport Destination { get; init; } = default!;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public CabinClass CabinClass { get; init; }
    public decimal BaseFare { get; init; }
    public bool IsInternational => Origin.CountryCode != Destination.CountryCode;
    public TimeSpan Duration => ArrivalTime - DepartureTime;
}
