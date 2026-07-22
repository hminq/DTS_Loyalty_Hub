using Core.Abstractions;
using Core.Entities;
using Core.Handlers;
using Core.Requests;
using FluentAssertions;
using Moq;

namespace Core.Tests.Handlers;

public sealed class ProcessExpiredCustomerTiersCommandHandlerTests
{
    private static readonly DateTime ProcessedAt = new(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ICustomerTierRepository> _repository = new();
    private readonly Mock<ICustomerTierMutationStore> _mutationStore = new();

    [Fact]
    public async Task Handle_CustomerAtMinimumTier_ResetsTierAndPointsForNewCycle()
    {
        var minimum = Tier(priority: 1, points: 0, cycleMonths: 6);
        var customer = Customer(minimum.TierConfigId, currentTierPoint: 150);
        var second = Tier(priority: 2, points: 500, cycleMonths: 6);
        Setup([minimum, second], [customer]);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.Should().Be(1);
        _mutationStore.Verify(store => store.ResetToMinTierAsync(
            customer.CustomerId,
            ProcessedAt,
            ProcessedAt.AddMonths(6),
            second.TierConfigId,
            second.PointsRequired,
            150,
            It.IsAny<CancellationToken>()), Times.Once);
        _mutationStore.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CustomerAtMinimumTierWithZeroTierPoint_StillStartsNewCycle()
    {
        var minimum = Tier(priority: 1, points: 0, cycleMonths: 12);
        var second = Tier(priority: 2, points: 500, cycleMonths: 6);
        var customer = Customer(minimum.TierConfigId, currentTierPoint: 0);
        Setup([minimum, second], [customer]);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.Should().Be(1);
        _mutationStore.Verify(store => store.ResetToMinTierAsync(
            customer.CustomerId,
            ProcessedAt,
            ProcessedAt.AddMonths(minimum.CycleMonth),
            second.TierConfigId,
            second.PointsRequired,
            0,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerAboveMinimumTier_DowngradesAndSetsCorrectNextTier()
    {
        var minimum = Tier(priority: 10, points: 0, cycleMonths: 12);
        var middle = Tier(priority: 20, points: 1_000, cycleMonths: 9);
        var highest = Tier(priority: 30, points: 5_000, cycleMonths: 6);
        var customer = Customer(highest.TierConfigId, currentTierPoint: 5_500);
        Setup([highest, minimum, middle], [customer]);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.Should().Be(1);
        _mutationStore.Verify(store => store.DowngradeTierAsync(
            customer.CustomerId,
            middle.TierConfigId,
            middle.PointsRequired,
            ProcessedAt,
            ProcessedAt.AddMonths(middle.CycleMonth),
            highest.TierConfigId,
            highest.PointsRequired,
            customer.CurrentTierPoint,
            middle.PointsRequired,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerAtSecondTier_SetsMinimumAsTargetAndSecondTierAsNext()
    {
        var minimum = Tier(priority: 1, points: 0, cycleMonths: 12);
        var second = Tier(priority: 2, points: 500, cycleMonths: 6);
        var customer = Customer(second.TierConfigId, currentTierPoint: 750);
        Setup([minimum, second], [customer]);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _mutationStore.Verify(store => store.DowngradeTierAsync(
            customer.CustomerId,
            minimum.TierConfigId,
            0,
            ProcessedAt,
            ProcessedAt.AddMonths(12),
            second.TierConfigId,
            500,
            750,
            0,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoExpiredCustomers_ReturnsZeroWithoutMutation()
    {
        Setup([Tier(1, 0, 12)], []);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.Should().Be(0);
        _mutationStore.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_MutationFails_PropagatesFailure()
    {
        var minimum = Tier(1, 0, 12);
        Setup([minimum], [Customer(minimum.TierConfigId, 100)]);
        _mutationStore
            .Setup(store => store.ResetToMinTierAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<Guid?>(), It.IsAny<decimal>(),
                It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("mutation failed"));

        var action = () => CreateHandler().Handle(Command(), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("mutation failed");
    }

    [Fact]
    public async Task Handle_CancelledBeforeProcessing_ThrowsWithoutMutation()
    {
        var minimum = Tier(1, 0, 12);
        Setup([minimum], [Customer(minimum.TierConfigId, 100)]);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var action = () => CreateHandler().Handle(Command(), cancellation.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        _mutationStore.VerifyNoOtherCalls();
    }

    private void Setup(
        IReadOnlyList<TierConfiguration> tiers,
        IReadOnlyList<ExpiredCustomerTier> customers)
    {
        _repository.Setup(repository => repository.GetTierConfigurationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tiers);
        _repository.Setup(repository => repository.GetExpiredCustomersAsync(ProcessedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);
    }

    private ProcessExpiredCustomerTiersCommandHandler CreateHandler() =>
        new(_repository.Object, _mutationStore.Object);

    private static ProcessExpiredCustomerTiersCommand Command() => new(ProcessedAt);

    private static TierConfiguration Tier(int priority, decimal points, int cycleMonths) =>
        new(Guid.NewGuid(), points, cycleMonths, priority);

    private static ExpiredCustomerTier Customer(Guid tierId, decimal currentTierPoint) =>
        new(Guid.NewGuid(), tierId, currentTierPoint);
}
