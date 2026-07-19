using Api.Mappers;
using Core.UseCases.Auth.Results;
using FluentAssertions;

namespace Api.Tests.Mappers;

public sealed class AuthMapperTests
{
    [Fact]
    public void ToResponseDto_AddsBearerTokenTypeAtApiBoundary()
    {
        var result = new LoginResult(
            "access-token",
            DateTime.UtcNow.AddMinutes(15),
            new AdminLoginResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "admin",
                "Admin User",
                Guid.NewGuid(),
                "System Admin"),
            []);

        var response = result.ToResponseDto();

        response.TokenType.Should().Be("Bearer");
    }
}
