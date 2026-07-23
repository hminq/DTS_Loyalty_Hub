using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

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
            DbUpdateException =>
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
