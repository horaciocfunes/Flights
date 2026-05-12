namespace SkyRoute.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class BookingRequestDto
{
    [Required] public string FlightId { get; init; } = string.Empty;
    [Required, MinLength(1)] public List<PassengerDto> Passengers { get; init; } = new();
}

public class PassengerDto
{
    [Required] public string FullName { get; init; } = string.Empty;
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    [Required] public string DocumentNumber { get; init; } = string.Empty;
}
