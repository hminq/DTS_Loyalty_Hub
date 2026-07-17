using Api.Dtos.Requests.Uploads;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Uploads;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Uploads.Commands;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Uploads;

[ApiController]
[Route("api/admin/uploads")]
[Authorize]
public sealed class UploadsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<UploadBannerRequestDto> _validator;

    public UploadsController(ISender sender, IValidator<UploadBannerRequestDto> validator)
    {
        _sender = sender;
        _validator = validator;
    }

    [HttpPost("banners")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = PermissionCodes.Media.Upload)]
    public async Task<ActionResult<ApiResponseDto<UploadBannerResponseDto>>> UploadBanner(
        [FromForm] UploadBannerRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        await using var content = request.File!.OpenReadStream();
        var result = await _sender.Send(new UploadBannerCommand(
            content,
            request.File.ContentType,
            request.Type), ct);

        return Ok(new ApiResponseDto<UploadBannerResponseDto>
        {
            Data = new UploadBannerResponseDto(result.Key)
        });
    }
}
