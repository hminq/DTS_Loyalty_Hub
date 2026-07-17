using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Tiers.Commands;
using Core.UseCases.Tiers.Handlers;
using Core.UseCases.Tiers.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Tiers;

public sealed class CreateTierCommandHandlerTests
{
    private readonly Mock<ITierRepository> _repository = new();
    private readonly Mock<IAuditLogWriter> _auditWriter = new();

    [Fact]
    public async Task Handle_ValidCommand_AddsTierAndAudit()
    {
        var command = new CreateTierCommand("Gold", 1000, 12, 2, Guid.NewGuid());
        _repository.Setup(x => x.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TierResult(Guid.NewGuid(), "Silver", 500, 12, 1)]);
        _repository.Setup(x => x.Add(It.IsAny<Tier>())).Returns((Tier tier) => tier);
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TierConfigId.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Gold");
        _repository.Verify(x => x.Add(It.IsAny<Tier>()), Times.Once);
        _auditWriter.Verify(x => x.Add(It.Is<AuditLogEntry>(entry =>
            entry.EntityType == AuditEntityTypes.TierConfig &&
            entry.EntityId == result.TierConfigId &&
            entry.ActorUserId == command.ActorUserId)), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsWithoutMutation()
    {
        _repository.Setup(x => x.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TierResult(Guid.NewGuid(), "gold", 500, 12, 1)]);
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new CreateTierCommand("Gold", 1000, 12, 2, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("TIER_NAME_ALREADY_EXISTS");
        VerifyNoMutation();
    }

    [Fact]
    public async Task Handle_DuplicatePriority_ThrowsWithoutMutation()
    {
        _repository.Setup(x => x.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TierResult(Guid.NewGuid(), "Silver", 500, 12, 2)]);
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new CreateTierCommand("Gold", 1000, 12, 2, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("TIER_PRIORITY_ALREADY_EXISTS");
        VerifyNoMutation();
    }

    [Fact]
    public async Task Handle_InvalidPointOrdering_ThrowsWithoutMutation()
    {
        _repository.Setup(x => x.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TierResult(Guid.NewGuid(), "Gold", 1000, 12, 2)]);
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new CreateTierCommand("Silver", 1500, 12, 1, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("TIER_POINTS_REQUIRED_INVALID");
        VerifyNoMutation();
    }

    private CreateTierCommandHandler CreateHandler() =>
        new(_repository.Object, _auditWriter.Object);

    private void VerifyNoMutation()
    {
        _repository.Verify(x => x.Add(It.IsAny<Tier>()), Times.Never);
        _auditWriter.Verify(x => x.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }
}
