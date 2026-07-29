using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Services;
using FluentAssertions;
using Moq;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class CampaignActionExecutorRegistryTests
{
    [Fact]
    public void Constructor_MatchingExecutor_ResolvesExecutor()
    {
        var executor = Executor("ISSUE_POINT", "CUSTOMER");

        var registry = new CampaignActionExecutorRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [executor.Object]);

        registry.GetRequired("ISSUE_POINT").Should().BeSameAs(executor.Object);
    }

    [Fact]
    public void Constructor_MissingExecutor_FailsStartupIntegrity()
    {
        var act = () => new CampaignActionExecutorRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            []);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Missing=[ISSUE_POINT]*");
    }

    [Fact]
    public void Constructor_WrongTargetKind_FailsStartupIntegrity()
    {
        var executor = Executor("ISSUE_POINT", "VOUCHER");

        var act = () => new CampaignActionExecutorRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [executor.Object]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Incompatible=[ISSUE_POINT]*");
    }

    [Fact]
    public void Constructor_DuplicateExecutor_FailsStartupIntegrity()
    {
        var act = () => new CampaignActionExecutorRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [
                Executor("ISSUE_POINT", "CUSTOMER").Object,
                Executor("ISSUE_POINT", "CUSTOMER").Object
            ]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duplicate campaign action executor registration*");
    }

    private static Mock<ICampaignActionExecutor> Executor(
        string actionType,
        string targetKind)
    {
        var executor = new Mock<ICampaignActionExecutor>();
        executor.SetupGet(item => item.ActionType).Returns(actionType);
        executor.SetupGet(item => item.RequiredTargetKind).Returns(targetKind);
        return executor;
    }
}
