namespace Api.Authentication;

public sealed class CurrentAdminContext : ICurrentAdminContext
{
    private Guid? _userId;
    private Guid? _adminId;
    private Guid? _adminSessionId;
    private Guid? _accessTokenJti;

    public Guid UserId => GetRequiredValue(_userId, nameof(UserId));

    public Guid AdminId => GetRequiredValue(_adminId, nameof(AdminId));

    public Guid AdminSessionId => GetRequiredValue(_adminSessionId, nameof(AdminSessionId));

    public Guid AccessTokenJti => GetRequiredValue(_accessTokenJti, nameof(AccessTokenJti));

    public void Set(
        Guid userId,
        Guid adminId,
        Guid adminSessionId,
        Guid accessTokenJti)
    {
        if (_userId.HasValue &&
            (_userId != userId ||
             _adminId != adminId ||
             _adminSessionId != adminSessionId ||
             _accessTokenJti != accessTokenJti))
        {
            throw new InvalidOperationException(
                "Current admin context cannot be changed during a request.");
        }

        _userId = userId;
        _adminId = adminId;
        _adminSessionId = adminSessionId;
        _accessTokenJti = accessTokenJti;
    }

    private static Guid GetRequiredValue(Guid? value, string propertyName)
    {
        return value ?? throw new InvalidOperationException(
            $"{propertyName} is unavailable because the permission policy has not established the current admin context.");
    }
}
