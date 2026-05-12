namespace SkyRoute.Domain.Entities;

using SkyRoute.Domain.Enums;

public class Booking
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string BookingReference { get; init; } = string.Empty;
    public string FlightId { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public Airport Origin { get; init; } = default!;
    public Airport Destination { get; init; } = default!;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public CabinClass CabinClass { get; init; }
    public decimal PricePerPassenger { get; init; }
    public decimal TotalPrice { get; init; }
    public List<Passenger> Passengers { get; init; } = new();
    public DateTime BookedAt { get; init; } = DateTime.UtcNow;
}
