namespace Core.Entities;

public static class PointResetAmounts
{
    public static decimal Calculate(decimal balanceBefore, decimal balanceAfter) =>
        Math.Abs(balanceAfter - balanceBefore);
}
