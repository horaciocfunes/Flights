namespace SkyRoute.Infrastructure.Repositories;

using System.Collections.Concurrent;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Interfaces;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<string, Booking> _store = new();

    public Task SaveAsync(Booking booking)
    {
        _store[booking.BookingReference] = booking;
        return Task.CompletedTask;
    }

    public Task<Booking?> GetByReferenceAsync(string reference)
    {
        _store.TryGetValue(reference, out var booking);
        return Task.FromResult(booking);
    }
}
