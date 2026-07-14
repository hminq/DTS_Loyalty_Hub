namespace Api.Authentication;

public sealed record CurrentCustomer(
    Guid UserId,
    Guid CustomerId,
    string Username);
