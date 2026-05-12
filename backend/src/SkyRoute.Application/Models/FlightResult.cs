namespace SkyRoute.Application.Models;

using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

public class FlightResult
{
    public string FlightId { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public Airport Origin { get; init; } = default!;
    public Airport Destination { get; init; } = default!;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public TimeSpan Duration { get; init; }
    public CabinClass CabinClass { get; init; }
    public decimal BaseFare { get; init; }
    public decimal PricePerPassenger { get; init; }
    public decimal TotalPrice { get; init; }
    public int PassengerCount { get; init; }
    public bool IsInternational { get; init; }
    public string DocumentLabel => IsInternational ? "Passport Number" : "National ID";
}
