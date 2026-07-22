using Core.Entities;
using FluentAssertions;

namespace Core.Tests.Entities;

public sealed class PointResetAmountsTests
{
    [Theory]
    [InlineData(200, 0, 200)]
    [InlineData(4000, 2500, 1500)]
    [InlineData(2500, 0, 2500)]
    [InlineData(0, 0, 0)]
    public void Calculate_ReturnsNonNegativeChangeMagnitude(
        decimal balanceBefore,
        decimal balanceAfter,
        decimal expectedAmount)
    {
        PointResetAmounts.Calculate(balanceBefore, balanceAfter)
            .Should().Be(expectedAmount);
    }
}
