namespace SkyRoute.Infrastructure.Pricing;

using SkyRoute.Domain.Interfaces;

public class BudgetWingsPricingStrategy : IPricingStrategy
{
    public const decimal MinimumPrice = 29.99m;

    public string ProviderName => "BudgetWings";

    // Discount applies only to the base fare. The floor is enforced after rounding.
    // e.g. 33.00 * 0.90 = 29.70 -> below floor -> 29.99
    //      33.33 * 0.90 = 29.997 -> rounds to 30.00 -> above floor -> 30.00
    public decimal Calculate(decimal baseFare)
    {
        var discounted = Math.Round(baseFare * 0.90m, 2, MidpointRounding.AwayFromZero);
        return Math.Max(discounted, MinimumPrice);
    }
}
