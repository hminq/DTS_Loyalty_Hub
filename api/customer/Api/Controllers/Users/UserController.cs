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

    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponseDto<PagedResponseDto<PointTransactionResponseDto>>>> GetTransactions(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = new Core.UseCases.Customers.Queries.GetPointTransactions.GetPointTransactionsQuery(CurrentCustomer.CustomerId, page, pageSize);
        var result = await _sender.Send(query, ct);

        if (result is null)
        {
            return Unauthorized(ApiErrorResponseDto.Create(
                "UNAUTHORIZED",
                "Invalid or missing customer ID claim."));
        }

        return Ok(new ApiResponseDto<PagedResponseDto<PointTransactionResponseDto>>
        {
            Data = new PagedResponseDto<PointTransactionResponseDto>
            {
                Items = result.Items.Select(x => x.ToResponseDto()),
                TotalCount = result.TotalCount,
                PageIndex = page,
                PageSize = pageSize
            }
        });
    }
}
