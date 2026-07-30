using Campaign.Contracts.Actions;
using Campaign.Contracts.Constants;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Exceptions;
using Consumer.Core.Services;
using FluentAssertions;
using Moq;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class IssuePointActionExecutorTests
{
    private readonly Mock<IIssuePointExecutionStore> _store = new();

    [Fact]
    public async Task Prepare_LocksDistinctResolvedCustomersInDeterministicOrder()
    {
        var firstCustomerId = Guid.Parse("10000000-0000-0000-0000-000000000000");
        var secondCustomerId = Guid.Parse("20000000-0000-0000-0000-000000000000");
        var executor = new IssuePointActionExecutor(_store.Object);
        var actions = new[]
        {
            Action(secondCustomerId, 50),
            Action(firstCustomerId, 100),
            Action(secondCustomerId, 200)
        };

        await executor.PrepareAsync(actions, CancellationToken.None);

        _store.Verify(store => store.LockCustomerPointsAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.SequenceEqual(new[] { firstCustomerId, secondCustomerId })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Execute_UsesOnlyResolvedCustomerAndTypedAmount()
    {
        var customerId = Guid.NewGuid();
        var action = Action(customerId, 125.50m);
        var context = new CampaignActionExecutionContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);
        var executor = new IssuePointActionExecutor(_store.Object);

        executor.Execute(action, context);

        _store.Verify(store => store.ApplyIssuePoint(
            It.Is<IssuePointMutation>(mutation =>
                mutation.CustomerId == customerId &&
                mutation.ActionId == action.ActionId &&
                mutation.EventId == context.EventId &&
                mutation.Amount == 125.50m)), Times.Once);
    }

    [Fact]
    public void Execute_UntypedParameters_RejectsWithoutMutation()
    {
        var valid = Action(Guid.NewGuid(), 50);
        var invalid = valid with { ParsedParameters = new object() };
        var executor = new IssuePointActionExecutor(_store.Object);

        var act = () => executor.Execute(
            invalid,
            new CampaignActionExecutionContext(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow));

        act.Should().Throw<CampaignConfigurationException>();
        _store.Verify(store => store.ApplyIssuePoint(
            It.IsAny<IssuePointMutation>()), Times.Never);
    }

    private static ResolvedCampaignAction Action(Guid customerId, decimal amount)
    {
        return new ResolvedCampaignAction(
            Guid.NewGuid(),
            ActionTypes.IssuePoint,
            1,
            "customer",
            CampaignTargetKinds.Customer,
            customerId,
            new IssuePointParameters(amount),
            null,
            null,
            0);
    }
}
