using Api.Mappers;
using Core.UseCases.Auth.Results;
using FluentAssertions;

namespace Api.Tests.Mappers;

public sealed class AuthMapperTests
{
    [Fact]
    public void LoginToResponseDto_AddsBearerTokenTypeAtApiBoundary()
    {
        var result = new LoginResult(
            "access-token",
            DateTime.UtcNow.AddMinutes(15),
            new CustomerLoginResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "customer",
                "Customer User"));

        result.ToResponseDto().TokenType.Should().Be("Bearer");
    }

    [Fact]
    public void RegisterToResponseDto_AddsBearerTokenTypeAtApiBoundary()
    {
        var result = new RegisterResult(
            "access-token",
            DateTime.UtcNow.AddMinutes(15),
            new CustomerRegisterResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "customer",
                "customer@example.com",
                "Customer User"));

        result.ToResponseDto().TokenType.Should().Be("Bearer");
    }
}
