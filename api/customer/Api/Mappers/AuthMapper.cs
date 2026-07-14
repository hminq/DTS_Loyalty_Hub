using Api.Dtos.Requests.Auth;
using Api.Dtos.Responses.Auth;
using Core.UseCases.Auth.Commands;
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
            TokenType = result.TokenType,
            ExpiresAt = result.ExpiresAt,
            Customer = new CustomerLoginResponseDto
            {
                UserId = result.Customer.UserId,
                CustomerId = result.Customer.CustomerId,
                Username = result.Customer.Username,
                FullName = result.Customer.FullName
            }
        };
    }

    public static RegisterCommand ToCommand(this RegisterRequestDto request)
    {
        return new RegisterCommand(
            request.Username!.Trim(),
            request.Email!.Trim(),
            request.Password!,
            request.FullName!.Trim(),
            request.Phone!.Trim());
    }

    public static RegisterResponseDto ToResponseDto(this RegisterResult result)
    {
        return new RegisterResponseDto
        {
            UserId = result.UserId,
            CustomerId = result.CustomerId,
            Username = result.Username,
            Email = result.Email,
            FullName = result.FullName
        };
    }
}
