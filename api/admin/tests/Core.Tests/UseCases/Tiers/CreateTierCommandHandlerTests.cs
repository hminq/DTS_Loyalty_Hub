using System.Text.Json;
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

public class CreateTierCommandHandlerTests
{
    private readonly Mock<ITierRepository> _tierRepository = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();
    private readonly CreateTierCommandHandler _sut;

    public CreateTierCommandHandlerTests()
    {
        _sut = new CreateTierCommandHandler(_tierRepository.Object, _auditLogWriter.Object);
    }

    private static CreateTierCommand CreateCommand(
        string name = "Gold",
        decimal pointsRequired = 1000,
        int cycleMonth = 12,
        int priority = 2,
        Guid? actorUserId = null)
    {
        return new CreateTierCommand(name, pointsRequired, cycleMonth, priority, actorUserId ?? Guid.NewGuid());
    }

    private void SetupExistingTiers(params TierResult[] tiers)
    {
        _tierRepository
            .Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tiers);
    }

    private void SetupAddReturnsSameTier()
    {
        _tierRepository
            .Setup(r => r.Add(It.IsAny<Tier>()))
            .Returns<Tier>(tier => tier);
    }

    // ---------- Happy path ----------

    [Fact]
    public async Task Handle_NoExistingTiers_CreatesTierAndReturnsResult()
    {
        // Arrange
        var command = CreateCommand(name: "Gold", pointsRequired: 1000, cycleMonth: 12, priority: 1);
        SetupExistingTiers();
        SetupAddReturnsSameTier();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Gold");
        result.PointsRequired.Should().Be(1000);
        result.CycleMonth.Should().Be(12);
        result.Priority.Should().Be(1);
        result.TierConfigId.Should().NotBeEmpty();

        _tierRepository.Verify(r => r.Add(It.Is<Tier>(t =>
            t.Name == "Gold" &&
            t.PointsRequired == 1000 &&
            t.CycleMonth == 12 &&
            t.Priority == 1)), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_WritesAuditLogWithCreatedTierSnapshot()
    {
        // Arrange
        var actorUserId = Guid.NewGuid();
        var command = CreateCommand(name: "Silver", pointsRequired: 500, cycleMonth: 6, priority: 1, actorUserId: actorUserId);
        SetupExistingTiers();
        SetupAddReturnsSameTier();

        AuditLogEntry? capturedEntry = null;
        _auditLogWriter.Setup(w => w.Add(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(entry => capturedEntry = entry);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _auditLogWriter.Verify(w => w.Add(It.IsAny<AuditLogEntry>()), Times.Once);
        capturedEntry.Should().NotBeNull();
        capturedEntry!.ActorUserId.Should().Be(actorUserId);
        capturedEntry.Action.Should().Be("CREATE");
        capturedEntry.EntityType.Should().Be(AuditEntityTypes.TierConfig);
        capturedEntry.EntityId.Should().Be(result.TierConfigId);
        capturedEntry.OldValue.Should().BeNull();
        capturedEntry.Metadata.Should().BeNull();

        // NewValue phải thực sự chứa đúng snapshot của tier vừa tạo, không chỉ "not null"
        using var doc = JsonDocument.Parse(capturedEntry.NewValue!);
        doc.RootElement.GetProperty("tierConfigId").GetGuid().Should().Be(result.TierConfigId);
        doc.RootElement.GetProperty("name").GetString().Should().Be("Silver");
        doc.RootElement.GetProperty("pointsRequired").GetDecimal().Should().Be(500);
        doc.RootElement.GetProperty("cycleMonth").GetInt32().Should().Be(6);
        doc.RootElement.GetProperty("priority").GetInt32().Should().Be(1);
    }

    // ---------- Entity-level validation (Tier.Create) short-circuits before touching repository ----------

    [Fact]
    public async Task Handle_InvalidTierName_ThrowsDomainExceptionWithoutQueryingRepository()
    {
        // Arrange: Tier.Create ném lỗi ngay, trước cả khi query existing tiers
        var command = CreateCommand(name: "");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("TIER_NAME_REQUIRED");

        _tierRepository.Verify(r => r.GetListAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tierRepository.Verify(r => r.Add(It.IsAny<Tier>()), Times.Never);
        _auditLogWriter.Verify(w => w.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    // ---------- Name conflict ----------

    [Fact]
    public async Task Handle_NameAlreadyExists_ThrowsDomainExceptionAndDoesNotPersist()
    {
        // Arrange
        var command = CreateCommand(name: "Gold", priority: 2, pointsRequired: 2000);
        SetupExistingTiers(new TierResult(Guid.NewGuid(), "Gold", 1000, 12, 1));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("TIER_NAME_ALREADY_EXISTS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Conflict);

        _tierRepository.Verify(r => r.Add(It.IsAny<Tier>()), Times.Never);
        _auditLogWriter.Verify(w => w.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NameAlreadyExistsWithDifferentCase_ThrowsDomainException()
    {
        // Arrange: so sánh tên phải case-insensitive ("gold" == "GOLD")
        var command = CreateCommand(name: "GOLD", priority: 2, pointsRequired: 2000);
        SetupExistingTiers(new TierResult(Guid.NewGuid(), "gold", 1000, 12, 1));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("TIER_NAME_ALREADY_EXISTS");
    }

    // ---------- Priority conflict ----------

    [Fact]
    public async Task Handle_PriorityAlreadyExists_ThrowsDomainExceptionAndDoesNotPersist()
    {
        // Arrange
        var command = CreateCommand(name: "Platinum", priority: 1, pointsRequired: 3000);
        SetupExistingTiers(new TierResult(Guid.NewGuid(), "Gold", 1000, 12, 1));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("TIER_PRIORITY_ALREADY_EXISTS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Conflict);

        _tierRepository.Verify(r => r.Add(It.IsAny<Tier>()), Times.Never);
    }

    // ---------- Points/priority ordering rule (2 nhánh của điều kiện OR — dễ bị bug nếu chỉ test 1 nhánh) ----------

    [Fact]
    public async Task Handle_HigherPriorityTierWithPointsNotGreaterThanLowerPriorityTier_ThrowsDomainException()
    {
        // Arrange: tier mới có priority CAO hơn (ưu tiên hơn) nhưng điểm yêu cầu lại <= tier priority thấp hơn
        // -> vi phạm "priority cao hơn phải cần nhiều điểm hơn"
        var command = CreateCommand(name: "Platinum", priority: 2, pointsRequired: 1000);
        SetupExistingTiers(new TierResult(Guid.NewGuid(), "Gold", 1000, 12, 1)); // priority thấp hơn (1) nhưng points bằng (1000)

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("TIER_POINTS_REQUIRED_INVALID");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Validation);

        _tierRepository.Verify(r => r.Add(It.IsAny<Tier>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LowerPriorityTierWithPointsNotLessThanHigherPriorityTier_ThrowsDomainException()
    {
        // Arrange: tier mới có priority THẤP hơn nhưng điểm yêu cầu lại >= tier priority cao hơn đã tồn tại
        var command = CreateCommand(name: "Bronze", priority: 1, pointsRequired: 1000);
        SetupExistingTiers(new TierResult(Guid.NewGuid(), "Gold", 1000, 12, 2)); // priority cao hơn (2) nhưng points bằng (1000)

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("TIER_POINTS_REQUIRED_INVALID");

        _tierRepository.Verify(r => r.Add(It.IsAny<Tier>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidPointsOrderAcrossMultipleExistingTiers_CreatesTierSuccessfully()
    {
        // Arrange: chèn tier mới vào đúng giữa 2 tier hiện có với thứ tự điểm hợp lệ
        // Bronze(priority=1, points=0) < NEW(priority=2, points=1000) < Gold(priority=3, points=2000)
        var command = CreateCommand(name: "Silver", priority: 2, pointsRequired: 1000);
        SetupExistingTiers(
            new TierResult(Guid.NewGuid(), "Bronze", 0, 12, 1),
            new TierResult(Guid.NewGuid(), "Gold", 2000, 12, 3));
        SetupAddReturnsSameTier();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert: không throw, tier được tạo và persist đúng
        result.Name.Should().Be("Silver");
        _tierRepository.Verify(r => r.Add(It.IsAny<Tier>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var command = CreateCommand();
        using var cts = new CancellationTokenSource();

        _tierRepository
            .Setup(r => r.GetListAsync(cts.Token))
            .ReturnsAsync([]);
        SetupAddReturnsSameTier();

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        _tierRepository.Verify(r => r.GetListAsync(cts.Token), Times.Once);
    }
}
