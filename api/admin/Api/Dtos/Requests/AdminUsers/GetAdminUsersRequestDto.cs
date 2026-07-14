namespace Api.Dtos.Requests.AdminUsers;

public sealed class GetAdminUsersRequestDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }

    public string? Status { get; set; }

    public Guid? RoleId { get; set; }
}
