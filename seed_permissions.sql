BEGIN;

INSERT INTO permissions (
    permission_id,
    code,
    name,
    group_code,
    group_name,
    action_code,
    action_name,
    group_sort_order,
    action_sort_order,
    created_at)
VALUES
    (gen_random_uuid(), 'campaign.view', 'View campaigns', 'campaign', 'Campaigns', 'view', 'View', 70, 10, now()),
    (gen_random_uuid(), 'campaign.create', 'Create campaigns', 'campaign', 'Campaigns', 'create', 'Create', 70, 20, now()),
    (gen_random_uuid(), 'campaign.update', 'Update campaigns', 'campaign', 'Campaigns', 'update', 'Update', 70, 30, now()),
    (gen_random_uuid(), 'campaign.delete', 'Delete campaigns', 'campaign', 'Campaigns', 'delete', 'Delete', 70, 40, now())
ON CONFLICT (code) DO UPDATE
SET
    name = EXCLUDED.name,
    group_code = EXCLUDED.group_code,
    group_name = EXCLUDED.group_name,
    action_code = EXCLUDED.action_code,
    action_name = EXCLUDED.action_name,
    group_sort_order = EXCLUDED.group_sort_order,
    action_sort_order = EXCLUDED.action_sort_order;

INSERT INTO role_permissions (
    role_permission_id,
    role_id,
    permission_id,
    created_at)
SELECT
    gen_random_uuid(),
    role.role_id,
    permission.permission_id,
    now()
FROM roles AS role
JOIN permissions AS permission
    ON permission.code IN (
        'campaign.view',
        'campaign.create',
        'campaign.update',
        'campaign.delete')
WHERE role.name = 'System Admin'
ON CONFLICT (role_id, permission_id) DO NOTHING;

COMMIT;
