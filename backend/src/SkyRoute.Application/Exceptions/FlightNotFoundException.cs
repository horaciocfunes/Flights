namespace SkyRoute.Application.Exceptions;

public class FlightNotFoundException : Exception
{
    public FlightNotFoundException(string flightId)
        : base($"Flight '{flightId}' was not found. Search results may have expired.") { }
}
