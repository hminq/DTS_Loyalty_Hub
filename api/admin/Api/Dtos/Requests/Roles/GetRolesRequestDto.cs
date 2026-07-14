namespace Api.Dtos.Requests.Roles;

public sealed class GetRolesRequestDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }
}
