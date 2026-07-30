-- Seed & migrate Notification permissions to grouped 'notification' code
-- Run this against the loyalty_hub database.

BEGIN;

-- 1. Remove old notification role_permissions bindings
DELETE FROM role_permissions
WHERE permission_id IN (
    SELECT permission_id FROM permissions 
    WHERE group_code IN ('notification_template', 'notification_log', 'notification_event_type', 'notification')
       OR code LIKE 'notification_template%' 
       OR code LIKE 'notification_log%' 
       OR code LIKE 'notification_event_type%'
);

-- 2. Delete old notification permissions
DELETE FROM permissions 
WHERE group_code IN ('notification_template', 'notification_log', 'notification_event_type', 'notification')
   OR code LIKE 'notification_template%' 
   OR code LIKE 'notification_log%' 
   OR code LIKE 'notification_event_type%';

-- 3. Insert grouped notification permissions with group_sort_order = 90
INSERT INTO permissions (permission_id, code, name, group_code, group_name, action_code, action_name, group_sort_order, action_sort_order, created_at)
VALUES 
    (gen_random_uuid(), 'notification.view', 'View notifications', 'notification', 'Notification', 'view', 'View', 90, 10, NOW()),
    (gen_random_uuid(), 'notification.create', 'Create notifications', 'notification', 'Notification', 'create', 'Create', 90, 20, NOW()),
    (gen_random_uuid(), 'notification.update', 'Update notifications', 'notification', 'Notification', 'update', 'Update', 90, 30, NOW()),
    (gen_random_uuid(), 'notification.delete', 'Delete notifications', 'notification', 'Notification', 'delete', 'Delete', 90, 40, NOW())
ON CONFLICT (code) DO UPDATE 
SET name = EXCLUDED.name,
    group_code = EXCLUDED.group_code,
    group_name = EXCLUDED.group_name,
    action_code = EXCLUDED.action_code,
    action_name = EXCLUDED.action_name,
    group_sort_order = EXCLUDED.group_sort_order,
    action_sort_order = EXCLUDED.action_sort_order;

-- 4. Grant all notification permissions to System Admin role
INSERT INTO role_permissions (role_permission_id, role_id, permission_id, created_at)
SELECT gen_random_uuid(), r.role_id, p.permission_id, NOW()
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'System Admin' AND p.group_code = 'notification'
ON CONFLICT (role_id, permission_id) DO NOTHING;

COMMIT;
