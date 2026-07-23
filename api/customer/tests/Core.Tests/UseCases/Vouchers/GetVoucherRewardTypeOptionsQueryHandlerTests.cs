using Core.Entities.Constants;
using Core.UseCases.Vouchers.Queries.GetVoucherRewardTypeOptions;
using FluentAssertions;

namespace Core.Tests.UseCases.Vouchers;

public sealed class GetVoucherRewardTypeOptionsQueryHandlerTests
{
    [Fact]
    public void VoucherRewardTypes_All_ReturnsDefinedValuesInStableOrder()
    {
        VoucherRewardTypes.All.Should().Equal(
            VoucherRewardTypes.Fixed,
            VoucherRewardTypes.Percent,
            VoucherRewardTypes.Gift);
        VoucherRewardTypes.All.Should().OnlyHaveUniqueItems();
        VoucherRewardTypes.All.Should().OnlyContain(value => VoucherRewardTypes.IsDefined(value));
    }

    [Fact]
    public async Task Handle_ReturnsVoucherRewardTypesAll()
    {
        var result = await new GetVoucherRewardTypeOptionsQueryHandler()
            .Handle(new GetVoucherRewardTypeOptionsQuery(), CancellationToken.None);

        result.Should().BeSameAs(VoucherRewardTypes.All);
    }
}
