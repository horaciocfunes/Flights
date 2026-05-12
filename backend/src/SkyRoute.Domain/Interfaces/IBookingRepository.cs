namespace SkyRoute.Domain.Interfaces;

using SkyRoute.Domain.Entities;

public interface IBookingRepository
{
    Task SaveAsync(Booking booking);
    Task<Booking?> GetByReferenceAsync(string reference);
}
