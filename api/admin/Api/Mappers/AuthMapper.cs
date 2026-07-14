using Api.Dtos.Requests.Auth;
using Api.Dtos.Responses.Auth;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Results;

namespace Api.Mappers;

public static class AuthMapper
{
    public static LoginCommand ToCommand(
        this LoginRequestDto request,
        string? ipAddress,
        string? userAgent)
    {
        return new LoginCommand(
            request.Username!.Trim(),
            request.Password!,
            ipAddress,
            userAgent);
    }

    public static LoginResponseDto ToResponseDto(this LoginResult result)
    {
        return new LoginResponseDto
        {
            AccessToken = result.AccessToken,
            TokenType = result.TokenType,
            ExpiresAt = result.ExpiresAt,
            Admin = new AdminLoginResponseDto
            {
                UserId = result.Admin.UserId,
                AdminId = result.Admin.AdminId,
                Username = result.Admin.Username,
                FullName = result.Admin.FullName,
                RoleId = result.Admin.RoleId,
                RoleName = result.Admin.RoleName
            },
            Permissions = result.Permissions
        };
    }
}
