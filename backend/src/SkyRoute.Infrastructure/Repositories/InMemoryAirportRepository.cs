namespace SkyRoute.Infrastructure.Repositories;

using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Interfaces;

public class InMemoryAirportRepository : IAirportRepository
{
    private static readonly List<Airport> Airports =
    [
        new("JFK", "John F. Kennedy International", "New York",     "United States", "US"),
        new("LAX", "Los Angeles International",     "Los Angeles",  "United States", "US"),
        new("ORD", "O'Hare International",          "Chicago",      "United States", "US"),
        new("MIA", "Miami International",           "Miami",        "United States", "US"),
        new("LHR", "Heathrow",                      "London",       "United Kingdom","GB"),
        new("MAD", "Adolfo Suárez Madrid–Barajas",  "Madrid",       "Spain",         "ES"),
        new("EZE", "Ministro Pistarini",            "Buenos Aires", "Argentina",     "AR"),
    ];

    public IEnumerable<Airport> GetAll() => Airports.AsReadOnly();

    public Airport? GetByCode(string code) =>
        Airports.FirstOrDefault(a => a.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}
