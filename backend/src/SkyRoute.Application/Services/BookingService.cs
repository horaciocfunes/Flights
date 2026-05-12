namespace SkyRoute.Application.Services;

using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using SkyRoute.Application.Exceptions;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Models;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;

public class BookingService : IBookingService
{
    private readonly IMemoryCache _cache;
    private readonly IBookingRepository _repository;
    private readonly BookingRequestValidator _validator;

    public BookingService(
        IMemoryCache cache,
        IBookingRepository repository,
        BookingRequestValidator validator)
    {
        _cache = cache;
        _repository = repository;
        _validator = validator;
    }

    public async Task<BookingConfirmation> BookAsync(BookingRequest request)
    {
        if (!_cache.TryGetValue(request.FlightId, out FlightResult? flight) || flight is null)
            throw new FlightNotFoundException(request.FlightId);

        if (request.Passengers.Count != flight.PassengerCount)
            throw new BookingValidationException(new[]
            {
                new ValidationFailure("Passengers",
                    $"Expected {flight.PassengerCount} passenger(s) for this flight but received {request.Passengers.Count}.")
            });

        // Enrich request with route context derived from the authoritative cached flight.
        var enrichedRequest = request with { IsInternational = flight.IsInternational };

        var validation = _validator.Validate(enrichedRequest);
        if (!validation.IsValid)
            throw new BookingValidationException(validation.Errors);

        var booking = new Booking
        {
            BookingReference = GenerateReference(),
            FlightId = flight.FlightId,
            FlightNumber = flight.FlightNumber,
            ProviderName = flight.ProviderName,
            Origin = flight.Origin,
            Destination = flight.Destination,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            CabinClass = flight.CabinClass,
            PricePerPassenger = flight.PricePerPassenger,
            TotalPrice = flight.TotalPrice,
            Passengers = request.Passengers
                .Select(p => new Passenger
                {
                    FullName = p.FullName,
                    Email = p.Email,
                    DocumentNumber = p.DocumentNumber,
                    DocumentType = flight.IsInternational ? DocumentType.Passport : DocumentType.NationalId
                })
                .ToList()
        };

        await _repository.SaveAsync(booking);

        return new BookingConfirmation
        {
            BookingReference = booking.BookingReference,
            TotalPrice = booking.TotalPrice,
            PassengerCount = booking.Passengers.Count
        };
    }

    private static string GenerateReference() =>
        "SKY-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
}
