using Api.Authentication;
using Api.Dtos.Requests.Users;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Users;
using Api.Mappers;
using Core.UseCases.Customers.Queries.GetPointTransactions;
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
        var result = await _sender.Send(new GetProfileAndWalletQuery
            (CurrentCustomer.CustomerId), ct);

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
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PointTransactionResponseDto>>>> GetTransactions(
        [FromQuery] PointTransactionFilterDto filter,
        CancellationToken ct = default)
    {
        var query = new GetPointTransactionsQuery(
            CurrentCustomer.CustomerId,
            filter.Page,
            filter.PageSize,
            filter.TransactionType,
            filter.FromDate,
            filter.ToDate,
            filter.MinAmount,
            filter.MaxAmount);
        var result = await _sender.Send(query, ct);

        if (result is null)
        {
            return Unauthorized(ApiErrorResponseDto.Create(
                "UNAUTHORIZED",
                "Invalid or missing customer ID claim."));
        }

        return Ok(new ApiResponseDto<IEnumerable<PointTransactionResponseDto>>
        {
            Data = result.Items.Select(x => x.ToResponseDto()),
            Meta = new ApiMetaDto
            {
                TotalItems = result.TotalCount,
                Page = result.PageIndex,
                PageSize = result.PageSize,
                TotalPages = result.PageSize > 0 ? (int)Math.Ceiling(result.TotalCount / (double)result.PageSize) : 0
            }
        });
    }
}
