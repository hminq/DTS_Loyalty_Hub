using Api.Dtos.Requests.Auth;
using Api.Dtos.Responses.Auth;
using Api.Constants;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Queries;
using Core.UseCases.Auth.Results;

namespace Api.Mappers;

public static class AuthMapper
{
    public static LoginCommand ToCommand(this LoginRequestDto request)
    {
        return new LoginCommand(
            request.Username!.Trim(),
            request.Password!);
    }

    public static LoginResponseDto ToResponseDto(this LoginResult result)
    {
        return new LoginResponseDto
        {
            AccessToken = result.AccessToken,
            TokenType = AuthTokenTypes.Bearer,
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

    public static GetCurrentAdminQuery ToQuery(
        Guid userId,
        Guid adminId,
        Guid adminSessionId,
        Guid accessTokenJti)
    {
        return new GetCurrentAdminQuery(
            userId,
            adminId,
            adminSessionId,
            accessTokenJti);
    }

    public static CurrentAdminResponseDto ToResponseDto(this CurrentAdminResult result)
    {
        return new CurrentAdminResponseDto
        {
            UserId = result.UserId,
            AdminId = result.AdminId,
            Username = result.Username,
            Email = result.Email,
            FullName = result.FullName,
            PhoneNumber = result.PhoneNumber,
            RoleId = result.RoleId,
            RoleName = result.RoleName,
            Status = result.Status,
            Permissions = result.Permissions
        };
    }
}
