using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.VoucherDefinitions.Commands;
using Core.UseCases.VoucherDefinitions.Handlers;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.VoucherDefinitions;

public sealed class CreateVoucherDefinitionCommandHandlerTests
{
    private readonly Mock<IVoucherDefinitionRepository> _repository = new();
    private readonly Mock<IAuditLogWriter> _auditWriter = new();

    [Fact]
    public async Task Handle_ValidCommand_AddsVoucherAndAudit()
    {
        var command = ValidCommand();
        _repository.Setup(x => x.ExistsByCodeAsync("WELCOME10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(x => x.Add(It.IsAny<VoucherDefinition>()))
            .Returns((VoucherDefinition voucher) => voucher);
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.VoucherDefinitionId.Should().NotBe(Guid.Empty);
        result.RemainingStock.Should().Be(command.TotalStock);
        _repository.Verify(x => x.Add(It.Is<VoucherDefinition>(voucher =>
            voucher.VoucherDefinitionId == result.VoucherDefinitionId)), Times.Once);
        _auditWriter.Verify(x => x.Add(It.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == command.ActorUserId &&
            entry.EntityType == AuditEntityTypes.VoucherDefinition &&
            entry.EntityId == result.VoucherDefinitionId &&
            entry.Action == "CREATE")), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsAndDoesNotMutate()
    {
        _repository.Setup(x => x.ExistsByCodeAsync("WELCOME10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();

        var action = () => handler.Handle(ValidCommand(), CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("VOUCHER_CODE_ALREADY_EXISTS");
        _repository.Verify(x => x.Add(It.IsAny<VoucherDefinition>()), Times.Never);
        _auditWriter.Verify(x => x.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidDomainData_DoesNotCallRepositoryOrAudit()
    {
        var command = ValidCommand() with { Name = "" };
        var handler = CreateHandler();

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<DomainException>();
        _repository.Verify(x => x.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repository.Verify(x => x.Add(It.IsAny<VoucherDefinition>()), Times.Never);
        _auditWriter.Verify(x => x.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    private CreateVoucherDefinitionCommandHandler CreateHandler() =>
        new(_repository.Object, _auditWriter.Object);

    private static CreateVoucherDefinitionCommand ValidCommand() => new(
        "WELCOME10", "Welcome voucher", null, null,
        VoucherRewardTypes.Fixed, 10, VoucherValidityTypes.Fixed,
        DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null,
        VoucherGenerationTypes.None, VoucherPublishTypes.Public, 100, Guid.NewGuid());
}
