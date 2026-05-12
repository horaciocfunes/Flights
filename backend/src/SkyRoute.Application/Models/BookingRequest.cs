namespace SkyRoute.Application.Models;

public record BookingRequest
{
    public string FlightId { get; init; } = string.Empty;
    public bool IsInternational { get; init; }
    public List<PassengerInfo> Passengers { get; init; } = new();
}
