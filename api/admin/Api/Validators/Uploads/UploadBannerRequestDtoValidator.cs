using Api.Dtos.Requests.Uploads;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.Uploads;

public sealed class UploadBannerRequestDtoValidator : AbstractValidator<UploadBannerRequestDto>
{
    public UploadBannerRequestDtoValidator()
    {
        RuleFor(request => request.Type)
            .Must(BannerUploadTypes.All.Contains)
            .WithErrorCode("BANNER_UPLOAD_TYPE_INVALID")
            .WithMessage("Banner type must be CAMPAIGN_BANNER or VOUCHER_DEFINITION_BANNER.")
            .OverridePropertyName("type");

        RuleFor(request => request.File)
            .NotNull()
            .WithErrorCode("BANNER_FILE_REQUIRED")
            .WithMessage("Banner file is required.")
            .OverridePropertyName("file");

        When(request => request.File is not null, () =>
        {
            RuleFor(request => request.File!.Length)
                .Must((request, fileSize) =>
                    fileSize > 0 && fileSize <= BannerUploadTypes.GetMaxFileSize(request.Type))
                .WithErrorCode("BANNER_FILE_SIZE_INVALID")
                .WithMessage("Banner file size exceeds the allowed limit for its type.")
                .OverridePropertyName("file");

            RuleFor(request => request.File!.ContentType)
                .Must(BannerFileTypes.All.Contains)
                .WithErrorCode("BANNER_FILE_TYPE_INVALID")
                .WithMessage("Banner file must be JPEG, PNG, or WebP.")
                .OverridePropertyName("file");
        });
    }
}
