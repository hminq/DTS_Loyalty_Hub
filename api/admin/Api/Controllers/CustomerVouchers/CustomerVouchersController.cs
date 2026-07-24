using Api.Dtos.Requests.CustomerVouchers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.CustomerVouchers;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerRedeemDetail;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerVoucherDetail;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.CustomerVouchers;

[ApiController]
[Route("api/admin")]
[Authorize]
public sealed class CustomerVouchersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<GetCustomerVouchersRequestDto> _getCustomerVouchersValidator;
    private readonly IValidator<GetCustomerRedeemsRequestDto> _getCustomerRedeemsValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public CustomerVouchersController(
        ISender sender,
        TimeProvider timeProvider,
        IValidator<GetCustomerVouchersRequestDto> getCustomerVouchersValidator,
        IValidator<GetCustomerRedeemsRequestDto> getCustomerRedeemsValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _timeProvider = timeProvider;
        _getCustomerVouchersValidator = getCustomerVouchersValidator;
        _getCustomerRedeemsValidator = getCustomerRedeemsValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet("customer-vouchers")]
    [Authorize(Policy = PermissionCodes.CustomerVouchers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CustomerVoucherResponseDto>>>> GetVouchers(
        [FromQuery] GetCustomerVouchersRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCustomerVouchersValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToQuery(_timeProvider.GetUtcNow().UtcDateTime),
            ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("customer-vouchers/{id:guid}")]
    [Authorize(Policy = PermissionCodes.CustomerVouchers.View)]
    public async Task<ActionResult<ApiResponseDto<CustomerVoucherDetailResponseDto>>> GetVoucherDetail(
        Guid id,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerVoucherDetailQuery(id), ct);

        return Ok(new ApiResponseDto<CustomerVoucherDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("customer-redeems")]
    [Authorize(Policy = PermissionCodes.CustomerVouchers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CustomerRedeemResponseDto>>>> GetRedeems(
        [FromQuery] GetCustomerRedeemsRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCustomerRedeemsValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("customer-redeems/{id:guid}")]
    [Authorize(Policy = PermissionCodes.CustomerVouchers.View)]
    public async Task<ActionResult<ApiResponseDto<CustomerRedeemDetailResponseDto>>> GetRedeemDetail(
        Guid id,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerRedeemDetailQuery(id), ct);

        return Ok(new ApiResponseDto<CustomerRedeemDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }
}
