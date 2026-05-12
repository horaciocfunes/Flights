namespace SkyRoute.Domain.Models;

using SkyRoute.Domain.Enums;

public record FlightSearchCriteria(
    string OriginCode,
    string DestinationCode,
    DateTime DepartureDate,
    int PassengerCount,
    CabinClass CabinClass
);
