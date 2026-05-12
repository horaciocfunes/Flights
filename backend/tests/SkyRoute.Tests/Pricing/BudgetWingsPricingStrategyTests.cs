namespace SkyRoute.Tests.Pricing;

using SkyRoute.Infrastructure.Pricing;
using Xunit;

public class BudgetWingsPricingStrategyTests
{
    private readonly BudgetWingsPricingStrategy _sut = new();

    private const decimal MinimumPrice = BudgetWingsPricingStrategy.MinimumPrice;

    [Fact]
    public void Calculate_WhenDiscountedPriceAboveMinimum_AppliesDiscountOnly()
    {
        var result = _sut.Calculate(200.00m); // 200 × 0.90 = 180.00

        Assert.Equal(180.00m, result);
    }

    [Fact]
    public void Calculate_WhenDiscountedPriceBelowMinimum_ReturnsMinimumPrice()
    {
        var result = _sut.Calculate(10.00m); // 10 × 0.90 = 9.00 → floor

        Assert.Equal(MinimumPrice, result);
    }

    [Fact]
    public void Calculate_WhenZeroBaseFare_ReturnsMinimumPrice()
    {
        var result = _sut.Calculate(0.00m);

        Assert.Equal(MinimumPrice, result);
    }

    [Theory]
    [InlineData(33.00, 29.99)] // 33.00 × 0.90 = 29.70 → below floor
    [InlineData(33.33, 30.00)] // 33.33 × 0.90 = 29.997 → rounds to 30.00 → above floor
    public void Calculate_AtMinimumBoundary_CorrectlyAppliesFloor(
        decimal baseFare, decimal expected)
    {
        var result = _sut.Calculate(baseFare);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_MinimumPriceIsBaseOfComparison_NotDiscountedMinimum()
    {
        // 29.99 × 0.90 = 26.991 → discounted price is below the floor
        // The 10% applies only to the base fare; the floor (29.99) is non-negotiable
        var result = _sut.Calculate(MinimumPrice);

        Assert.Equal(MinimumPrice, result);
    }

    [Theory]
    [InlineData(33.32, 29.99)] // 33.32 × 0.90 = 29.988 → rounds to 29.99 (AwayFromZero) — floor not triggered
    [InlineData(33.31, 29.99)] // 33.31 × 0.90 = 29.979 → rounds to 29.98 — floor triggered
    public void Calculate_AtPreciseBoundary_CorrectlyAppliesOrSkipsFloor(
        decimal baseFare, decimal expected)
    {
        var result = _sut.Calculate(baseFare);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ProviderName_IsBudgetWings()
    {
        Assert.Equal("BudgetWings", _sut.ProviderName);
    }
}
