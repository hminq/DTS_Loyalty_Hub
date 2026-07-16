namespace Api.Authentication;

public interface ICurrentAdminContext
{
    Guid UserId { get; }

    Guid AdminId { get; }

    Guid AdminSessionId { get; }

    Guid AccessTokenJti { get; }
}
