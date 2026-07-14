using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IAccessTokenService
{
    DateTime CreateExpiresAt();

    AccessToken CreateAccessToken(CustomerTokenUser user, DateTime expiresAt);
}
