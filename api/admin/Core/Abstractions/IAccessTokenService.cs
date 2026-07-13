using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IAccessTokenService
{
    AccessToken CreateAccessToken(AdminLoginUser user);
}
