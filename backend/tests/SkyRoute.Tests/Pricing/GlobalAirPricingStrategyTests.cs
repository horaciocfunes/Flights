namespace SkyRoute.Tests.Pricing;

using SkyRoute.Infrastructure.Pricing;
using Xunit;

public class GlobalAirPricingStrategyTests
{
    private readonly GlobalAirPricingStrategy _sut = new();

    [Theory]
    [InlineData(100.00, 115.00)] // 100 × 1.15 = 115.00
    [InlineData(200.00, 230.00)] // 200 × 1.15 = 230.00
    [InlineData(10.43,   11.99)] // 10.43 × 1.15 = 11.9945 → 3rd decimal 4 < 5 → rounds down
    [InlineData(10.44,   12.01)] // 10.44 × 1.15 = 12.006  → 3rd decimal 6 ≥ 5 → rounds up
    [InlineData(0.00,     0.00)] // zero base fare
    public void Calculate_WithVariousFares_AppliesSurchargeAndRoundsToTwoDecimals(
        decimal baseFare, decimal expected)
    {
        var result = _sut.Calculate(baseFare);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_RoundingAppliedAfterSurcharge_NotBefore()
    {
        // Correct:  Round(10.435 × 1.15, 2) = Round(12.00025, 2) = 12.00
        // Incorrect: Round(10.435, 2) × 1.15 = 10.44 × 1.15 = 12.006 → 12.01
        var result = _sut.Calculate(10.435m);

        Assert.Equal(12.00m, result);
    }

    [Fact]
    public void ProviderName_IsGlobalAir()
    {
        Assert.Equal("GlobalAir", _sut.ProviderName);
    }
}
