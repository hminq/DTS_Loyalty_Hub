namespace Core.UseCases.Auth.Models;

public sealed record NewCustomerUser(
    string Username,
    string Email,
    string PasswordHash,
    string FullName,
    string Phone);
