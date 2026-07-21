using Api.Dtos.Responses;
using Api.Dtos.Responses.Permissions;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Permissions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Permissions;

[ApiController]
[Route("api/admin/permissions")]
[Authorize(Policy = PermissionCodes.Roles.View)]
public sealed class PermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public PermissionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<PermissionGroupResponseDto>>>> Get(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetPermissionsQuery(), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<PermissionGroupResponseDto>>
        {
            Data = result.ToResponseDto()
        });
    }
}
