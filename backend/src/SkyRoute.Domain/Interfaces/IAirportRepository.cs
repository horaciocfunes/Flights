namespace SkyRoute.Domain.Interfaces;

using SkyRoute.Domain.Entities;

public interface IAirportRepository
{
    IEnumerable<Airport> GetAll();
    Airport? GetByCode(string code);
}
