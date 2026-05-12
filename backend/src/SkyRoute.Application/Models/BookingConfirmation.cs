namespace SkyRoute.Application.Models;

public class BookingConfirmation
{
    public string BookingReference { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public int PassengerCount { get; init; }
}
