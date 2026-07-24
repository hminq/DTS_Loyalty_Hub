namespace Core.Exceptions;

public sealed class VoucherPoolImportException : Exception
{
    public VoucherPoolImportException(
        string errorCode,
        bool retriable = false,
        int? rowNumber = null)
        : base(errorCode)
    {
        ErrorCode = errorCode;
        Retriable = retriable;
        RowNumber = rowNumber;
    }

    public string ErrorCode { get; }
    public bool Retriable { get; }
    public int? RowNumber { get; }
}
