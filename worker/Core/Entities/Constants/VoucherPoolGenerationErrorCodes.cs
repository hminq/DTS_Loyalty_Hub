namespace Core.Entities.Constants;

public static class VoucherPoolGenerationErrorCodes
{
    public const string StateInvalid = "VOUCHER_POOL_GENERATION_STATE_INVALID";
    public const string CodeCollision = "VOUCHER_POOL_GENERATION_CODE_COLLISION";
    public const string DatabaseError = "VOUCHER_POOL_GENERATION_DATABASE_ERROR";
    public const string UnexpectedError = "VOUCHER_POOL_GENERATION_UNEXPECTED_ERROR";
    public const string ImportStateInvalid = "VOUCHER_POOL_IMPORT_STATE_INVALID";
    public const string ImportFileNotFound = "VOUCHER_POOL_IMPORT_FILE_NOT_FOUND";
    public const string ImportFileTooLarge = "VOUCHER_POOL_IMPORT_FILE_TOO_LARGE";
    public const string ImportHeaderInvalid = "VOUCHER_POOL_IMPORT_HEADER_INVALID";
    public const string ImportCsvInvalid = "VOUCHER_POOL_IMPORT_CSV_INVALID";
    public const string ImportCodeEmpty = "VOUCHER_POOL_IMPORT_CODE_EMPTY";
    public const string ImportCodeTooLong = "VOUCHER_POOL_IMPORT_CODE_TOO_LONG";
    public const string ImportDuplicateInFile = "VOUCHER_POOL_IMPORT_DUPLICATE_IN_FILE";
    public const string ImportCodeAlreadyExists = "VOUCHER_POOL_IMPORT_CODE_ALREADY_EXISTS";
    public const string ImportCountMismatch = "VOUCHER_POOL_IMPORT_COUNT_MISMATCH";
    public const string ImportS3Error = "VOUCHER_POOL_IMPORT_S3_ERROR";
    public const string ImportDatabaseError = "VOUCHER_POOL_IMPORT_DATABASE_ERROR";
    public const string ImportUnexpectedError = "VOUCHER_POOL_IMPORT_UNEXPECTED_ERROR";
}
