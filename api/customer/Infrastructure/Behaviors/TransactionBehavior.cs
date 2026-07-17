using Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly LoyaltyHubDbContext _dbContext;

    public TransactionBehavior(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest)
        {
            return await next();
        }

        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await next();
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var response = await next();

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return response;
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                throw;
            }
        });
    }

}
