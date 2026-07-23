using Core.Entities;

namespace Core.Abstractions;

public interface IVoucherPoolGenerationFailureClassifier
{
    VoucherPoolGenerationFailure Classify(Exception exception);
}
