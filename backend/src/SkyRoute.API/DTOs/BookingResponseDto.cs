namespace SkyRoute.API.DTOs;

public class BookingResponseDto
{
    public string BookingReference { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public int PassengerCount { get; init; }
}
