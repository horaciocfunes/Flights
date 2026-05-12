namespace SkyRoute.Application.Interfaces;

using SkyRoute.Application.Models;

public interface IBookingService
{
    Task<BookingConfirmation> BookAsync(BookingRequest request);
}
