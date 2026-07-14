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

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponseDto<UserProfileAndWalletResponseDto>>> GetProfile(CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("customer_id")?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized(ApiErrorResponseDto.Create(
                "UNAUTHORIZED",
                "Invalid or missing customer ID claim."));
        }

        var query = new GetProfileAndWalletQuery(customerId);
        var result = await _sender.Send(query, ct);

        if (result is null)
        {
            return NotFound(ApiErrorResponseDto.Validation([
                new ApiValidationErrorDto
                {
                    Field = "CustomerId",
                    Code = "CUSTOMER_NOT_FOUND",
                    Message = "Customer profile was not found."
                }
            ]));
        }

        return Ok(new ApiResponseDto<UserProfileAndWalletResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }
}
