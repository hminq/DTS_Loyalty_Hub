using Api.Authentication;
using Api.Controllers.Auth;
using Api.Dtos.Requests.Auth;
using Api.Localization;
using Api.Mappers;
using Core.UseCases.Auth.Commands;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Controllers.Auth;

public sealed class AuthControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidator = new();
    private readonly Mock<IAuthenticatedAdminSessionAccessor> _adminSessionAccessor = new();

    [Fact]
    public async Task Logout_AuthenticatedSession_SendsCommandAndReturnsNoContent()
    {
        var session = new AuthenticatedAdminSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        _adminSessionAccessor.Setup(accessor => accessor.TryGet(out session))
            .Returns(true);
        var controller = CreateController();

        var result = await controller.Logout(CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _sender.Verify(sender => sender.Send(
            It.Is<LogoutCommand>(command =>
                command.AdminId == session.AdminId &&
                command.AdminSessionId == session.AdminSessionId &&
                command.AccessTokenJti == session.AccessTokenJti),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_MissingAuthenticatedSession_ReturnsUnauthorizedWithoutSendingCommand()
    {
        var session = default(AuthenticatedAdminSession)!;
        _adminSessionAccessor.Setup(accessor => accessor.TryGet(out session))
            .Returns(false);
        var controller = CreateController();

        var result = await controller.Logout(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
        _sender.Verify(sender => sender.Send(
            It.IsAny<LogoutCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private AuthController CreateController() => new(
        _sender.Object,
        _loginValidator.Object,
        CreateValidationErrorMapper(),
        _adminSessionAccessor.Object);

    private static ValidationErrorMapper CreateValidationErrorMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<ApiMessageResolver>();
        services.AddScoped<ValidationErrorMapper>();
        return services.BuildServiceProvider().GetRequiredService<ValidationErrorMapper>();
    }
}
