using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class Permission
{
    public Guid PermissionId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string ActionCode { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public int GroupSortOrder { get; set; }

    public int ActionSortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
