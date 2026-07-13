using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IAccessTokenService
{
    AccessToken CreateAccessToken(CustomerLoginUser user);
}
