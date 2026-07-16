using Core.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Authentication;

public sealed class ApiJwtBearerEvents : JwtBearerEvents
{
    public override Task Challenge(JwtBearerChallengeContext context)
    {
        // Chặn JwtBearerHandler tự ghi 401 trơn (không body) vào response
        context.HandleResponse();

        throw new DomainException(
            "UNAUTHORIZED",
            "Access token is missing or invalid.",
            DomainErrorType.Unauthorized);
    }

    public override Task Forbidden(ForbiddenContext context)
    {
        // ForbiddenContext không có HandleResponse() (khác JwtBearerChallengeContext).
        // Throw exception ở đây chặn luôn code set 403 mặc định phía sau nên không cần gọi gì thêm.
        throw new DomainException(
            "FORBIDDEN",
            "You do not have permission to access this resource.",
            DomainErrorType.Forbidden);
    }
}