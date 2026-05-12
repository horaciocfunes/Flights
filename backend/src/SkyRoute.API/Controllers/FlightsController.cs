namespace SkyRoute.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SkyRoute.API.DTOs;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Models;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IFlightSearchService _searchService;

    public FlightsController(IFlightSearchService searchService) =>
        _searchService = searchService;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] FlightSearchRequestDto dto)
    {
        var criteria = new FlightSearchCriteria(
            dto.OriginCode,
            dto.DestinationCode,
            dto.DepartureDate,
            dto.PassengerCount,
            dto.CabinClass);

        var results = await _searchService.SearchAsync(criteria);

        var response = results.Select(f => new FlightResponseDto
        {
            FlightId         = f.FlightId,
            FlightNumber     = f.FlightNumber,
            ProviderName     = f.ProviderName,
            Origin           = new AirportDto { Code = f.Origin.Code, Name = f.Origin.Name, City = f.Origin.City, Country = f.Origin.Country },
            Destination      = new AirportDto { Code = f.Destination.Code, Name = f.Destination.Name, City = f.Destination.City, Country = f.Destination.Country },
            DepartureTime    = f.DepartureTime,
            ArrivalTime      = f.ArrivalTime,
            DurationMinutes  = (int)f.Duration.TotalMinutes,
            CabinClass       = f.CabinClass.ToString(),
            PricePerPassenger= f.PricePerPassenger,
            TotalPrice       = f.TotalPrice,
            PassengerCount   = f.PassengerCount,
            IsInternational  = f.IsInternational,
            DocumentLabel    = f.DocumentLabel
        });

        return Ok(response);
    }
}
