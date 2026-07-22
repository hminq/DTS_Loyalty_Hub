using Api.Authentication;
using Api.Dtos.Requests.CustomerUsers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.CustomerUsers;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.CustomerUsers.Commands;
using Core.UseCases.CustomerUsers.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.CustomerUsers;

[ApiController]
[Route("api/admin/customer-users")]
[Authorize]
public sealed class CustomerUsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<GetCustomerUsersRequestDto> _getCustomerUsersValidator;
    private readonly IValidator<GetCustomerUserHistoryRequestDto> _getCustomerUserHistoryValidator;
    private readonly IValidator<UpdateCustomerUserRequestDto> _updateCustomerUserValidator;
    private readonly IValidator<UpdateCustomerUserStatusRequestDto> _updateCustomerUserStatusValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public CustomerUsersController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetCustomerUsersRequestDto> getCustomerUsersValidator,
        IValidator<GetCustomerUserHistoryRequestDto> getCustomerUserHistoryValidator,
        IValidator<UpdateCustomerUserRequestDto> updateCustomerUserValidator,
        IValidator<UpdateCustomerUserStatusRequestDto> updateCustomerUserStatusValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getCustomerUsersValidator = getCustomerUsersValidator;
        _getCustomerUserHistoryValidator = getCustomerUserHistoryValidator;
        _updateCustomerUserValidator = updateCustomerUserValidator;
        _updateCustomerUserStatusValidator = updateCustomerUserStatusValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.CustomerUsers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CustomerUserListItemResponseDto>>>> GetList(
        [FromQuery] GetCustomerUsersRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCustomerUsersValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("{customerId:guid}")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.View)]
    public async Task<ActionResult<ApiResponseDto<CustomerUserResponseDto>>> GetById(
        Guid customerId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerUserByIdQuery(customerId), ct);

        return Ok(new ApiResponseDto<CustomerUserResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("{customerId:guid}/points")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.View)]
    public async Task<ActionResult<ApiResponseDto<CustomerUserPointsResponseDto>>> GetPoints(
        Guid customerId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerUserPointsQuery(customerId), ct);

        return Ok(new ApiResponseDto<CustomerUserPointsResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("{customerId:guid}/vouchers")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CustomerVoucherResponseDto>>>> GetVouchers(
        Guid customerId,
        [FromQuery] GetCustomerUserHistoryRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCustomerUserHistoryValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new GetCustomerUserVouchersQuery(customerId, request.Page, request.PageSize),
            ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("{customerId:guid}/voucher-redemptions")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CustomerVoucherRedemptionResponseDto>>>> GetVoucherRedemptions(
        Guid customerId,
        [FromQuery] GetCustomerUserHistoryRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCustomerUserHistoryValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new GetCustomerUserVoucherRedemptionsQuery(customerId, request.Page, request.PageSize),
            ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("{customerId:guid}/point-transactions")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CustomerPointTransactionResponseDto>>>> GetPointTransactions(
        Guid customerId,
        [FromQuery] GetCustomerUserHistoryRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCustomerUserHistoryValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new GetCustomerUserPointTransactionsQuery(customerId, request.Page, request.PageSize),
            ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.Update)]
    public async Task<ActionResult<ApiResponseDto<CustomerUserResponseDto>>> Update(
        Guid customerId,
        [FromBody] UpdateCustomerUserRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _updateCustomerUserValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToCommand(customerId, _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<CustomerUserResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPatch("{customerId:guid}/status")]
    [Authorize(Policy = PermissionCodes.CustomerUsers.Disable)]
    public async Task<IActionResult> UpdateStatus(
        Guid customerId,
        [FromBody] UpdateCustomerUserStatusRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _updateCustomerUserStatusValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        await _sender.Send(
            request.ToCommand(customerId, _currentAdminContext.UserId),
            ct);

        return NoContent();
    }
}
