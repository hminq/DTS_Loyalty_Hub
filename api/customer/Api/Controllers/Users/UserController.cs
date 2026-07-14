using Api.Authentication;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Users;
using Api.Mappers;
using Core.UseCases.Customers.Queries.GetProfileAndWallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UserController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentCustomerAccessor _currentCustomerAccessor;

    public UserController(
        ISender sender,
        ICurrentCustomerAccessor currentCustomerAccessor)
    {
        _sender = sender;
        _currentCustomerAccessor = currentCustomerAccessor;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponseDto<UserProfileAndWalletResponseDto>>> GetProfile(CancellationToken ct)
    {
        if (!_currentCustomerAccessor.TryGetCurrentCustomer(out var currentCustomer) || currentCustomer is null)
        {
            return Unauthorized(ApiErrorResponseDto.Create(
                "UNAUTHORIZED",
                "Invalid or missing customer token."));
        }

        var result = await _sender.Send(
            new GetProfileAndWalletQuery(currentCustomer.CustomerId),
            ct);

        if (result is null)
        {
            return NotFound(ApiErrorResponseDto.Create(
                "CUSTOMER_NOT_FOUND",
                "Customer profile was not found."));
        }

        return Ok(new ApiResponseDto<UserProfileAndWalletResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }
}
