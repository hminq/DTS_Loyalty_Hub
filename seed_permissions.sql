BEGIN;

WITH seed_permissions (
    code,
    name,
    group_code,
    group_name,
    group_sort_order,
    action_sort_order
) AS (
    VALUES
        ('role.view', 'View Role', 'role', 'Role', 10, 10),
        ('role.create', 'Create Role', 'role', 'Role', 10, 20),
        ('role.update', 'Update Role', 'role', 'Role', 10, 30),
        ('role.delete', 'Delete Role', 'role', 'Role', 10, 40),

        ('permission.view', 'View Permission', 'permission', 'Permission', 20, 10),

        ('role_permission.view', 'View Role Permission', 'role_permission', 'Role Permission', 30, 10),
        ('role_permission.update', 'Update Role Permission', 'role_permission', 'Role Permission', 30, 20),

        ('admin_user.view', 'View Admin User', 'admin_user', 'Admin User', 40, 10),
        ('admin_user.create', 'Create Admin User', 'admin_user', 'Admin User', 40, 20),
        ('admin_user.update', 'Update Admin User', 'admin_user', 'Admin User', 40, 30),
        ('admin_user.disable', 'Disable Admin User', 'admin_user', 'Admin User', 40, 40),
        ('admin_user.reset_password', 'Reset Admin User Password', 'admin_user', 'Admin User', 40, 50),

        ('customer_user.view', 'View Customer User', 'customer_user', 'Customer User', 50, 10),
        ('customer_user.create', 'Create Customer User', 'customer_user', 'Customer User', 50, 20),
        ('customer_user.update', 'Update Customer User', 'customer_user', 'Customer User', 50, 30),
        ('customer_user.disable', 'Disable Customer User', 'customer_user', 'Customer User', 50, 40),
        ('customer_user.reset_password', 'Reset Customer User Password', 'customer_user', 'Customer User', 50, 50)
)
INSERT INTO permissions (
    code,
    name,
    group_code,
    group_name,
    group_sort_order,
    action_sort_order
)
SELECT
    code,
    name,
    group_code,
    group_name,
    group_sort_order,
    action_sort_order
FROM seed_permissions
ON CONFLICT (code) DO UPDATE SET
    name = EXCLUDED.name,
    group_code = EXCLUDED.group_code,
    group_name = EXCLUDED.group_name,
    group_sort_order = EXCLUDED.group_sort_order,
    action_sort_order = EXCLUDED.action_sort_order;

INSERT INTO roles (name)
VALUES ('System Admin')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT
    roles.role_id,
    permissions.permission_id
FROM roles
CROSS JOIN permissions
WHERE roles.name = 'System Admin'
  AND permissions.code IN (
      'role.view',
      'role.create',
      'role.update',
      'role.delete',
      'permission.view',
      'role_permission.view',
      'role_permission.update',
      'admin_user.view',
      'admin_user.create',
      'admin_user.update',
      'admin_user.disable',
      'admin_user.reset_password',
      'customer_user.view',
      'customer_user.create',
      'customer_user.update',
      'customer_user.disable',
      'customer_user.reset_password'
  )
ON CONFLICT (role_id, permission_id) DO NOTHING;

COMMIT;
