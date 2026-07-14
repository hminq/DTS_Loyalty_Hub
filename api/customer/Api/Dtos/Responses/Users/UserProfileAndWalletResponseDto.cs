namespace Api.Dtos.Responses.Users;

public sealed class UserProfileAndWalletResponseDto
{
    public UserProfileResponseDto Profile { get; set; } = null!;
    public UserWalletResponseDto Wallet { get; set; } = null!;
}

public sealed class UserProfileResponseDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TierName { get; set; }
    public decimal CurrentTierPoint { get; set; }
    public decimal NextTierPoint { get; set; }
}

public sealed class UserWalletResponseDto
{
    public decimal ActivePoint { get; set; }
    public decimal LockedPoint { get; set; }
    public decimal LifetimePoint { get; set; }
    public decimal SpentPoint { get; set; }
    public decimal ExpiredPoint { get; set; }
}
