using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities;
using Scheduler.Core.Entities.Constants;
using Scheduler.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Scheduler.Infrastructure.Implementations;

public sealed class VoucherPoolGenerationFailureClassifier
    : IVoucherPoolGenerationFailureClassifier
{
    public VoucherPoolGenerationFailure Classify(Exception exception)
    {
        return exception switch
        {
            VoucherPoolGenerationException generationException =>
                new VoucherPoolGenerationFailure(
                    generationException.ErrorCode,
                    generationException.Retriable),
            VoucherPoolImportException importException =>
                new VoucherPoolGenerationFailure(
                    importException.ErrorCode,
                    importException.Retriable),
            DbUpdateException or NpgsqlException =>
                new VoucherPoolGenerationFailure(
                    VoucherPoolGenerationErrorCodes.DatabaseError,
                    true),
            _ =>
                new VoucherPoolGenerationFailure(
                    VoucherPoolGenerationErrorCodes.UnexpectedError,
                    true)
        };
    }
}
