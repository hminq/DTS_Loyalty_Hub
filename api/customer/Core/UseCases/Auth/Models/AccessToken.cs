namespace Core.UseCases.Auth.Models;

public sealed record AccessToken(string Value, DateTime ExpiresAt);
