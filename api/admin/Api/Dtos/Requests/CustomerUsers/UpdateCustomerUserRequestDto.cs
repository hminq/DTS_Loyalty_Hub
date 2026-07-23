namespace Api.Dtos.Requests.CustomerUsers;

public sealed class UpdateCustomerUserRequestDto
{
    public string? Email { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }
}
