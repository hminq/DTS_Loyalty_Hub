using Core.Abstractions;
using Core.Entities;
using Core.Handlers;
using Core.Requests;
using FluentAssertions;
using Moq;

namespace Core.Tests.Handlers;

public sealed class ProcessExpiredCustomerTiersCommandHandlerTests
{
    private const int BatchSize = 100;
    private static readonly DateTime ProcessedAt = new(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ICustomerTierRepository> _repository = new();
    private readonly Mock<ICustomerTierMutationStore> _mutationStore = new();

    [Fact]
    public async Task Handle_CustomerAtMinimumTier_CreatesNewCycleMutation()
    {
        var minimum = Tier(priority: 1, points: 0, cycleMonths: 6);
        var second = Tier(priority: 2, points: 500, cycleMonths: 6);
        var customer = Customer(minimum.TierConfigId, currentTierPoint: 150);
        Setup([minimum, second], [customer]);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.Should().Be(new ProcessExpiredCustomerTierBatchResult(1, 1));
        _mutationStore.Verify(store => store.ApplyBatchAsync(
            It.Is<IReadOnlyList<CustomerTierExpirationMutation>>(mutations =>
                mutations.Count == 1 &&
                mutations[0].CustomerId == customer.CustomerId &&
                mutations[0].TierConfigId == minimum.TierConfigId &&
                mutations[0].CurrentTierPoint == 0 &&
                mutations[0].StartTier == ProcessedAt &&
                mutations[0].ExpiredTier == ProcessedAt.AddMonths(6) &&
                mutations[0].NextTierConfigId == second.TierConfigId &&
                mutations[0].NextTierPoint == second.PointsRequired &&
                mutations[0].TierPointBefore == 150),
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerAboveMinimumTier_CreatesDowngradeMutation()
    {
        var minimum = Tier(priority: 10, points: 0, cycleMonths: 12);
        var middle = Tier(priority: 20, points: 1_000, cycleMonths: 9);
        var highest = Tier(priority: 30, points: 5_000, cycleMonths: 6);
        var customer = Customer(highest.TierConfigId, currentTierPoint: 5_500);
        Setup([highest, minimum, middle], [customer]);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.ProcessedCount.Should().Be(1);
        _mutationStore.Verify(store => store.ApplyBatchAsync(
            It.Is<IReadOnlyList<CustomerTierExpirationMutation>>(mutations =>
                mutations.Count == 1 &&
                mutations[0].TierConfigId == middle.TierConfigId &&
                mutations[0].CurrentTierPoint == middle.PointsRequired &&
                mutations[0].ExpiredTier == ProcessedAt.AddMonths(middle.CycleMonth) &&
                mutations[0].NextTierConfigId == highest.TierConfigId),
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoExpiredCustomers_ReturnsEmptyBatch()
    {
        Setup([Tier(1, 0, 12)], []);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        result.Should().Be(new ProcessExpiredCustomerTierBatchResult(0, 0));
        _mutationStore.Verify(store => store.ApplyBatchAsync(
            It.Is<IReadOnlyList<CustomerTierExpirationMutation>>(mutations => mutations.Count == 0),
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingTierConfiguration_ThrowsWithoutMutation()
    {
        Setup([Tier(1, 0, 12)], [Customer(Guid.NewGuid(), 100)]);

        var action = () => CreateHandler().Handle(Command(), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*references missing tier configuration*");
        _mutationStore.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_InvalidBatchSize_ThrowsBeforeQuery()
    {
        var action = () => CreateHandler().Handle(
            new ProcessExpiredCustomerTierBatchCommand(ProcessedAt, 0),
            CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
        _repository.VerifyNoOtherCalls();
        _mutationStore.VerifyNoOtherCalls();
    }

    private void Setup(
        IReadOnlyList<TierConfiguration> tiers,
        IReadOnlyList<ExpiredCustomerTier> customers)
    {
        _repository.Setup(repository => repository.GetTierConfigurationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tiers);
        _repository.Setup(repository => repository.GetExpiredCustomersAsync(
                ProcessedAt,
                BatchSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);
    }

    private ProcessExpiredCustomerTierBatchCommandHandler CreateHandler() =>
        new(_repository.Object, _mutationStore.Object);

    private static ProcessExpiredCustomerTierBatchCommand Command() => new(ProcessedAt, BatchSize);

    private static TierConfiguration Tier(int priority, decimal points, int cycleMonths) =>
        new(Guid.NewGuid(), points, cycleMonths, priority);

    private static ExpiredCustomerTier Customer(Guid tierId, decimal currentTierPoint) =>
        new(Guid.NewGuid(), tierId, currentTierPoint);
}
