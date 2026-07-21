using Api.Controllers.AuditLogs;
using Api.Dtos.Requests.AuditLogs;
using Api.Dtos.Responses;
using Api.Dtos.Responses.AuditLogs;
using Api.Mappers;
using Core.UseCases.AuditLogs.Queries;
using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Tests.Controllers.AuditLogs;

public sealed class AuditLogsControllerTests
{
    [Fact]
    public async Task GetList_ReturnsPagedRowsWithActorFields()
    {
        var actorUserId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(instance => instance.Send(
                It.IsAny<GetAuditLogsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AuditLogResult>(
                [new AuditLogResult(
                    Guid.NewGuid(), actorUserId, "admin", "System Admin", "UPDATE", "Role",
                    Guid.NewGuid(), "{}", "{}", "{}", DateTime.UtcNow)],
                1, 20, 1));
        var validator = new Mock<IValidator<GetAuditLogsRequestDto>>();
        validator.Setup(instance => instance.ValidateAsync(
                It.IsAny<GetAuditLogsRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var controller = new AuditLogsController(sender.Object, validator.Object, null!);

        var actionResult = await controller.GetList(new GetAuditLogsRequestDto(), CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<IReadOnlyCollection<AuditLogResponseDto>>>()
            .Which;
        response.Data.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            ActorUserId = (Guid?)actorUserId,
            ActorUsername = "admin",
            ActorFullName = "System Admin"
        });
        response.Meta.Should().BeEquivalentTo(new { Page = 1, PageSize = 20, TotalItems = 1, TotalPages = 1 });
    }

    [Fact]
    public async Task GetFilterOptions_ReturnsStandardResponseWrapper()
    {
        var sender = new Mock<ISender>();
        sender.Setup(instance => instance.Send(
                It.IsAny<GetAuditLogFilterOptionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuditLogFilterOptionsResult(["Admin", "Role"], ["CREATE", "UPDATE"]));
        var controller = new AuditLogsController(
            sender.Object,
            Mock.Of<IValidator<GetAuditLogsRequestDto>>(),
            null!);

        var actionResult = await controller.GetFilterOptions(CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should().BeOfType<ApiResponseDto<AuditLogFilterOptionsResponseDto>>().Which;
        response.Data.EntityTypes.Should().Equal("Admin", "Role");
        response.Data.Actions.Should().Equal("CREATE", "UPDATE");
        response.Meta.Should().BeNull();
    }

    [Theory]
    [InlineData(nameof(AuditLogsController.GetList))]
    [InlineData(nameof(AuditLogsController.GetFilterOptions))]
    public void Endpoint_RequiresAuditLogViewPolicy(string methodName)
    {
        var attribute = typeof(AuditLogsController).GetMethod(methodName)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        attribute.Policy.Should().Be("audit_log.view");
    }
}
