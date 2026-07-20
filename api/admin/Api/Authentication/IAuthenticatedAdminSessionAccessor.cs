namespace Api.Authentication;

public interface IAuthenticatedAdminSessionAccessor
{
    bool TryGet(out AuthenticatedAdminSession session);
}
