namespace Api.Dtos.Requests.CustomerUsers;

public sealed class GetCustomerUsersRequestDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }

    public string? Status { get; set; }

    public Guid? TierId { get; set; }
}
