namespace Core.Abstractions;

public interface IAdminRepository
{
    void Add(Guid adminId, Guid userId, Guid roleId, DateTime createdAt);

    Task UpdateRoleAsync(Guid adminId, Guid roleId, CancellationToken ct = default);
}
