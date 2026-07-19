using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Exceptions;

namespace Api.Authentication;

public sealed class CurrentCustomerAccessor : ICurrentCustomerAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentCustomerAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool TryGetCurrentCustomer(out CurrentCustomer? customer)
    {
        customer = null;

        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (!TryReadGuid(principal, JwtRegisteredClaimNames.Sub, out var userId) ||
            !TryReadGuid(principal, "customer_id", out var customerId))
        {
            return false;
        }

        var username = principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;
        customer = new CurrentCustomer(userId, customerId, username);
        return true;
    }

    public CurrentCustomer GetRequiredCustomer()
    {
        if (!TryGetCurrentCustomer(out var customer) || customer is null)
        {
            throw new DomainException(
                "UNAUTHORIZED",
                DomainErrorType.Unauthorized);
        }

        return customer;
    }

    private static bool TryReadGuid(ClaimsPrincipal principal, string claimType, out Guid value)
    {
        var rawValue = principal.FindFirst(claimType)?.Value;
        return Guid.TryParse(rawValue, out value);
    }
}
