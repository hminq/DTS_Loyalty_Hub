namespace Core.Entities.Constants;

public static class VoucherPoolGenerationLimits
{
    public const int MaxAttempts = 3;
    public const int MaxCollisionRounds = 5;
    public const int MaxImportedCount = 100_000;
}
