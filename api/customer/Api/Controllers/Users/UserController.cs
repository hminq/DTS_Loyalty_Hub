using Api.Authentication;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Users;
using Api.Mappers;
using Core.UseCases.Customers.Queries.GetProfileAndWallet;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Users;

[ApiController]
[Route("api/users")]
public sealed class UserController : CustomerControllerBase
{
    private readonly ISender _sender;

    public UserController(
        ISender sender,
        ICurrentCustomerAccessor currentCustomerAccessor)
        : base(currentCustomerAccessor)
    {
        _sender = sender;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponseDto<UserProfileAndWalletResponseDto>>> GetProfile(CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetProfileAndWalletQuery(CurrentCustomer.CustomerId),
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
