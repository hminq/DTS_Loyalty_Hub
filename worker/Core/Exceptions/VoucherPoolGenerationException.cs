namespace Core.Exceptions;

public sealed class VoucherPoolGenerationException : Exception
{
    public VoucherPoolGenerationException(
        string errorCode,
        bool retriable = false)
        : base(errorCode)
    {
        ErrorCode = errorCode;
        Retriable = retriable;
    }

    public string ErrorCode { get; }

    public bool Retriable { get; }
}
