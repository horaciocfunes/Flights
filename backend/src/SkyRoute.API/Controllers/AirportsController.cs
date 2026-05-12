namespace SkyRoute.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SkyRoute.API.DTOs;
using SkyRoute.Domain.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AirportsController : ControllerBase
{
    private readonly IAirportRepository _airports;

    public AirportsController(IAirportRepository airports) => _airports = airports;

    [HttpGet]
    public IActionResult GetAll()
    {
        var airports = _airports.GetAll()
            .Select(a => new AirportDto
            {
                Code    = a.Code,
                Name    = a.Name,
                City    = a.City,
                Country = a.Country
            });

        return Ok(airports);
    }
}
