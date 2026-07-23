using Api.Authentication;
using Api.Dtos.Requests.Vouchers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Vouchers;
using Api.Mappers;
using Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.Vouchers.Queries.GetCustomerVouchers;
using Core.UseCases.Vouchers.Queries.GetVoucherRewardTypeOptions;
using Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Vouchers;

[ApiController]
[Route("api/users")]
public sealed class VouchersController : CustomerControllerBase
{
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<CustomerVoucherFilterDto> _customerVoucherFilterValidator;
    private readonly IValidator<VoucherRedemptionFilterDto> _voucherRedemptionFilterValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public VouchersController(
        ISender sender,
        TimeProvider timeProvider,
        IValidator<CustomerVoucherFilterDto> customerVoucherFilterValidator,
        IValidator<VoucherRedemptionFilterDto> voucherRedemptionFilterValidator,
        ValidationErrorMapper validationErrorMapper,
        ICurrentCustomerAccessor currentCustomerAccessor)
        : base(currentCustomerAccessor)
    {
        _sender = sender;
        _timeProvider = timeProvider;
        _customerVoucherFilterValidator = customerVoucherFilterValidator;
        _voucherRedemptionFilterValidator = voucherRedemptionFilterValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet("vouchers")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CustomerVoucherResponseDto>>>> GetVouchers(
        [FromQuery] CustomerVoucherFilterDto filter,
        CancellationToken ct = default)
    {
        var validationResult = await _customerVoucherFilterValidator.ValidateAsync(filter, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new GetCustomerVouchersQuery(
                CurrentCustomer.CustomerId,
                filter.Page,
                filter.PageSize,
                filter.Name,
                filter.VoucherDefRewardType,
                filter.RedeemAtFrom,
                filter.RedeemAtTo,
                _timeProvider.GetUtcNow().UtcDateTime),
            ct);

        return Ok(CreatePagedResponse(
            result.Items.Select(item => item.ToResponseDto()),
            result.PageIndex,
            result.PageSize,
            result.TotalCount));
    }

    [HttpGet("vouchers/reward-type-options")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<string>>>> GetRewardTypeOptions(
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetVoucherRewardTypeOptionsQuery(), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<string>>
        {
            Data = result
        });
    }

    [HttpGet("vouchers/{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<CustomerVoucherDetailResponseDto>>> GetVoucherDetail(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new GetCustomerVoucherDetailQuery(CurrentCustomer.CustomerId, id),
            ct);

        return Ok(new ApiResponseDto<CustomerVoucherDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("redeems")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<VoucherRedemptionResponseDto>>>> GetRedeems(
        [FromQuery] VoucherRedemptionFilterDto filter,
        CancellationToken ct = default)
    {
        var validationResult = await _voucherRedemptionFilterValidator.ValidateAsync(filter, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new GetVoucherRedemptionsQuery(
                CurrentCustomer.CustomerId,
                filter.Page,
                filter.PageSize,
                filter.Name,
                filter.VoucherDefRewardType,
                filter.RedeemAtFrom,
                filter.RedeemAtTo,
                filter.CampaignName),
            ct);

        return Ok(CreatePagedResponse(
            result.Items.Select(item => item.ToResponseDto()),
            result.PageIndex,
            result.PageSize,
            result.TotalCount));
    }

    private static ApiResponseDto<IEnumerable<T>> CreatePagedResponse<T>(
        IEnumerable<T> items,
        int page,
        int pageSize,
        int totalItems)
    {
        return new ApiResponseDto<IEnumerable<T>>
        {
            Data = items,
            Meta = new ApiMetaDto
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = pageSize > 0
                    ? (int)Math.Ceiling(totalItems / (double)pageSize)
                    : 0
            }
        };
    }
}
