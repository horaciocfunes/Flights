namespace SkyRoute.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Services;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Interfaces;
using SkyRoute.Infrastructure.Pricing;
using SkyRoute.Infrastructure.Providers;
using SkyRoute.Infrastructure.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSkyRouteServices(this IServiceCollection services)
    {
        services.AddSingleton<IAirportRepository, InMemoryAirportRepository>();
        services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();

        services.AddSingleton<GlobalAirPricingStrategy>();
        services.AddSingleton<BudgetWingsPricingStrategy>();

        // All IFlightProvider registrations are resolved as IEnumerable<IFlightProvider>
        // in FlightSearchService — adding a new provider here is the only change required.
        services.AddSingleton<IFlightProvider, GlobalAirProvider>();
        services.AddSingleton<IFlightProvider, BudgetWingsProvider>();

        services.AddScoped<IFlightSearchService, FlightSearchService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<BookingRequestValidator>();

        return services;
    }
}
