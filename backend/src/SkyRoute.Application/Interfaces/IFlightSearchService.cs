namespace SkyRoute.Application.Interfaces;

using SkyRoute.Application.Models;
using SkyRoute.Domain.Models;

public interface IFlightSearchService
{
    Task<IEnumerable<FlightResult>> SearchAsync(FlightSearchCriteria criteria);
}
