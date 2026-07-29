using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface IVoucherPoolGenerationFailureClassifier
{
    VoucherPoolGenerationFailure Classify(Exception exception);
}
