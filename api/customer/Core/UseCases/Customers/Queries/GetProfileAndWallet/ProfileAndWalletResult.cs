namespace Core.UseCases.Customers.Queries.GetProfileAndWallet;

public sealed record ProfileAndWalletResult(
    UserProfileResult Profile,
    UserWalletResult Wallet);

public sealed record UserProfileResult(
    string Username,
    string Email,
    string? FullName,
    string? PhoneNumber,
    string? TierName,
    decimal CurrentTierPoint,
    decimal NextTierPoint);

public sealed record UserWalletResult(
    decimal ActivePoint,
    decimal LockedPoint,
    decimal LifetimePoint,
    decimal SpentPoint,
    decimal ExpiredPoint);
