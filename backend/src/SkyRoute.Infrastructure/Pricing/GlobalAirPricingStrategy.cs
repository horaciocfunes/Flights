namespace SkyRoute.Infrastructure.Pricing;

using SkyRoute.Domain.Interfaces;

public class GlobalAirPricingStrategy : IPricingStrategy
{
    public string ProviderName => "GlobalAir";

    // Rounding is applied after the surcharge, never before.
    // Round(10.435 * 1.15, 2) = Round(12.00025, 2) = 12.00
    // vs Round(10.44, 2) * 1.15 = 10.44 * 1.15 = 12.006 -> 12.01 (wrong order)
    public decimal Calculate(decimal baseFare) =>
        Math.Round(baseFare * 1.15m, 2, MidpointRounding.AwayFromZero);
}
