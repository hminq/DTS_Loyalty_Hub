using Api.Dtos.Requests.VoucherDefinitions;
using FluentValidation;

namespace Api.Validators.VoucherDefinitions;

public sealed class CreateVoucherPoolImportJobRequestDtoValidator
    : AbstractValidator<CreateVoucherPoolImportJobRequestDto>
{
    public CreateVoucherPoolImportJobRequestDtoValidator()
    {
        RuleFor(request => request.ImportFileKey)
            .NotEmpty()
            .WithErrorCode("VOUCHER_POOL_IMPORT_FILE_REQUIRED")
            .OverridePropertyName("importFileKey");
    }
}
