namespace Core.UseCases.CustomerVouchers.Results;

public sealed record CustomerInfoResult(
    Guid CustomerId,
    string CustomerUsername,
    string CustomerEmail,
    string? CustomerPhone);
