namespace Api.Dtos.Requests.Roles;

public sealed record GetRoleOptionsRequestDto
{
    public string? Keyword { get; init; }
}
