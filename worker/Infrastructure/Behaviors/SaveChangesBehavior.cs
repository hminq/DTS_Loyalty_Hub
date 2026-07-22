using Core.Abstractions;
using MediatR;
using Persistence.Models.Context;

namespace Infrastructure.Behaviors;

public sealed class SaveChangesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly LoyaltyHubDbContext _dbContext;

    public SaveChangesBehavior(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IWriteRequest || request is ITransactionalRequest)
        {
            return await next();
        }

        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await next();
        }

        try
        {
            var response = await next();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return response;
        }
        catch
        {
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
