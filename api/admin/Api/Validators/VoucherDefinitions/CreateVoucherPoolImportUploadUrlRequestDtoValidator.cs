using Api.Dtos.Requests.VoucherDefinitions;
using FluentValidation;

namespace Api.Validators.VoucherDefinitions;

public sealed class CreateVoucherPoolImportUploadUrlRequestDtoValidator
    : AbstractValidator<CreateVoucherPoolImportUploadUrlRequestDto>
{
    public CreateVoucherPoolImportUploadUrlRequestDtoValidator()
    {
        RuleFor(request => request.FileName)
            .NotEmpty()
            .WithErrorCode("VOUCHER_POOL_IMPORT_FILE_REQUIRED")
            .OverridePropertyName("fileName");

        RuleFor(request => request.FileSizeBytes)
            .GreaterThan(0)
            .WithErrorCode("VOUCHER_POOL_IMPORT_FILE_SIZE_INVALID")
            .OverridePropertyName("fileSizeBytes");
    }
}
