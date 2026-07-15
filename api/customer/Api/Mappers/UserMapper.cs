using Api.Dtos.Responses.Users;
using Core.UseCases.Customers.Queries.GetProfileAndWallet;

namespace Api.Mappers;

public static class UserMapper
{
    public static UserProfileAndWalletResponseDto ToResponseDto(this ProfileAndWalletResult result)
    {
        return new UserProfileAndWalletResponseDto
        {
            Profile = new UserProfileResponseDto
            {
                Username = result.Profile.Username,
                Email = result.Profile.Email,
                FullName = result.Profile.FullName,
                PhoneNumber = result.Profile.PhoneNumber,
                TierName = result.Profile.TierName,
                CurrentTierPoint = result.Profile.CurrentTierPoint,
                NextTierPoint = result.Profile.NextTierPoint
            },
            Wallet = new UserWalletResponseDto
            {
                ActivePoint = result.Wallet.ActivePoint,
                LockedPoint = result.Wallet.LockedPoint,
                LifetimePoint = result.Wallet.LifetimePoint,
                SpentPoint = result.Wallet.SpentPoint,
                ExpiredPoint = result.Wallet.ExpiredPoint
            }
        };
    }
    public static PointTransactionResponseDto ToResponseDto(this Core.UseCases.Customers.Queries.GetPointTransactions.PointTransactionResult result)
    {
        return new PointTransactionResponseDto
        {
            Id = result.Id,
            TransactionType = result.TransactionType,
            Amount = result.Amount,
            BalanceBefore = result.BalanceBefore,
            BalanceAfter = result.BalanceAfter,
            CreatedAt = result.CreatedAt,
            SourceEventId = result.SourceEventId
        };
    }
}
