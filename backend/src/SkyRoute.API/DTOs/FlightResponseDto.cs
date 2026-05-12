namespace SkyRoute.API.DTOs;

public class FlightResponseDto
{
    public string FlightId { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public AirportDto Origin { get; init; } = default!;
    public AirportDto Destination { get; init; } = default!;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public int DurationMinutes { get; init; }
    public string CabinClass { get; init; } = string.Empty;
    public decimal PricePerPassenger { get; init; }
    public decimal TotalPrice { get; init; }
    public int PassengerCount { get; init; }
    public bool IsInternational { get; init; }
    public string DocumentLabel { get; init; } = string.Empty;
}

public class AirportDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}
