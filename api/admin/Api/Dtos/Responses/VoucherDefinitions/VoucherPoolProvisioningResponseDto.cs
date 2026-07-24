using System.Text.Json;

namespace Api.Dtos.Responses.VoucherDefinitions;

public sealed class VoucherPoolProvisioningResponseDto
{
    public Guid JobId { get; set; }

    public string JobType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int ExpectedCount { get; set; }

    public int ProcessedCount { get; set; }

    public int AttemptCount { get; set; }

    public string? ErrorCode { get; set; }

    public JsonElement? ErrorDetails { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
