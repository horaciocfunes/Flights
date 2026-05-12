namespace SkyRoute.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SkyRoute.API.DTOs;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Models;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService) =>
        _bookingService = bookingService;

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookingRequestDto dto)
    {
        var request = new BookingRequest
        {
            FlightId   = dto.FlightId,
            Passengers = dto.Passengers
                .Select(p => new PassengerInfo
                {
                    FullName       = p.FullName,
                    Email          = p.Email,
                    DocumentNumber = p.DocumentNumber
                })
                .ToList()
        };

        var confirmation = await _bookingService.BookAsync(request);

        return StatusCode(201, new BookingResponseDto
        {
            BookingReference = confirmation.BookingReference,
            TotalPrice       = confirmation.TotalPrice,
            PassengerCount   = confirmation.PassengerCount
        });
    }
}
