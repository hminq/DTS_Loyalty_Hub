namespace Core.UseCases.Auth.Models;

public sealed record ReferralCustomer(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string Status);
