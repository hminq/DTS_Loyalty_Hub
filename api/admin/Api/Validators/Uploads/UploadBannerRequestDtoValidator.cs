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
            .OverridePropertyName("type");

        RuleFor(request => request.File)
            .NotNull()
            .WithErrorCode("BANNER_FILE_REQUIRED")
            .OverridePropertyName("file");

        When(request => request.File is not null, () =>
        {
            RuleFor(request => request.File!.Length)
                .Must((request, fileSize) =>
                    fileSize > 0 && fileSize <= BannerUploadTypes.GetMaxFileSize(request.Type))
                .WithErrorCode("BANNER_FILE_SIZE_INVALID")
                .OverridePropertyName("file");

            RuleFor(request => request.File!.ContentType)
                .Must(BannerFileTypes.All.Contains)
                .WithErrorCode("BANNER_FILE_TYPE_INVALID")
                .OverridePropertyName("file");
        });
    }
}
