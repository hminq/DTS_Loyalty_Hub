namespace Api.Dtos.Requests.VoucherDefinitions;

public sealed class GetVoucherDefinitionsRequestDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }

    public string? RewardType { get; set; }

    public string? ValidityType { get; set; }

    public string? PublishType { get; set; }
}
