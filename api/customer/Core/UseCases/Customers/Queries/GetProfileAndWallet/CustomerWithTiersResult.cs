namespace Core.UseCases.Customers.Queries.GetProfileAndWallet;

public sealed record CustomerWithTiersResult(
    string? FullName,
    string? CurrentTierName,
    decimal CurrentTierPoint,
    string? NextTierName,
    decimal NextTierPoint);
