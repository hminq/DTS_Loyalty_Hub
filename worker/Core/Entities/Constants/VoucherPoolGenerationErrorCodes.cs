namespace Core.Entities.Constants;

public static class VoucherPoolGenerationErrorCodes
{
    public const string StateInvalid = "VOUCHER_POOL_GENERATION_STATE_INVALID";
    public const string CodeCollision = "VOUCHER_POOL_GENERATION_CODE_COLLISION";
    public const string DatabaseError = "VOUCHER_POOL_GENERATION_DATABASE_ERROR";
    public const string UnexpectedError = "VOUCHER_POOL_GENERATION_UNEXPECTED_ERROR";
}
