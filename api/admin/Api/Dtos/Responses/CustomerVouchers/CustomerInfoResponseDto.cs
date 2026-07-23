namespace Api.Dtos.Responses.CustomerVouchers;

public sealed class CustomerInfoResponseDto
{
    public Guid CustomerId { get; init; }

    public string CustomerUsername { get; init; } = string.Empty;

    public string CustomerEmail { get; init; } = string.Empty;

    public string? CustomerPhone { get; init; }
}
