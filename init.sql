/* CREATE DATABASE dts_loyalty_hub */

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL,
    password_hash TEXT NOT NULL,
    full_name VARCHAR(50),
    phone_number VARCHAR(15),
    user_type VARCHAR(25) NOT NULL,
    status VARCHAR(25) NOT NULL DEFAULT 'ENABLE',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT uq_users_phone_number UNIQUE (phone_number),
    CONSTRAINT ck_users_user_type CHECK (user_type IN ('ADMIN', 'CUSTOMER')),
    CONSTRAINT ck_users_status CHECK (status IN ('ENABLE', 'DISABLE'))
);

CREATE TABLE roles (
    role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_roles_name UNIQUE (name)
);

CREATE TABLE permissions (
    permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(100) NOT NULL,
    name VARCHAR(100) NOT NULL,
    group_code VARCHAR(50) NOT NULL,
    group_name VARCHAR(100) NOT NULL,
    action_code VARCHAR(50) NOT NULL,
    action_name VARCHAR(100) NOT NULL,
    group_sort_order INTEGER NOT NULL DEFAULT 0,
    action_sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_permissions_code UNIQUE (code),
    CONSTRAINT uq_permissions_group_action UNIQUE (group_code, action_code),
    CONSTRAINT ck_permissions_code_group_action
        CHECK (code = group_code || '.' || action_code)
);

CREATE TABLE role_permissions (
    role_permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role_id UUID NOT NULL,
    permission_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_role_permissions_role
        FOREIGN KEY (role_id) REFERENCES roles (role_id) ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_permission
        FOREIGN KEY (permission_id) REFERENCES permissions (permission_id) ON DELETE CASCADE,
    CONSTRAINT uq_role_permissions_role_permission UNIQUE (role_id, permission_id)
);

CREATE TABLE admin (
    admin_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_admin_user
        FOREIGN KEY (user_id) REFERENCES users (user_id) ON DELETE CASCADE,
    CONSTRAINT fk_admin_role
        FOREIGN KEY (role_id) REFERENCES roles (role_id),
    CONSTRAINT uq_admin_user UNIQUE (user_id)
);

CREATE TABLE tiers_config (
    tier_config_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL,
    points_required NUMERIC(18, 2) NOT NULL DEFAULT 0,
    cycle_month INTEGER NOT NULL DEFAULT 3,
    priority INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_tiers_config_name UNIQUE (name),
    CONSTRAINT uq_tiers_config_priority UNIQUE (priority),
    CONSTRAINT ck_tiers_config_points_required CHECK (points_required >= 0),
    CONSTRAINT ck_tiers_config_cycle_month CHECK (cycle_month > 0)
);

CREATE TABLE customer (
    customer_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    tier_id UUID,
    current_tier_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    next_tier_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_customer_user
        FOREIGN KEY (user_id) REFERENCES users (user_id) ON DELETE CASCADE,
    CONSTRAINT fk_customer_tier
        FOREIGN KEY (tier_id) REFERENCES tiers_config (tier_config_id),
    CONSTRAINT uq_customer_user UNIQUE (user_id),
    CONSTRAINT ck_customer_current_tier_point CHECK (current_tier_point >= 0),
    CONSTRAINT ck_customer_next_tier_point CHECK (next_tier_point >= 0)
);

CREATE TABLE customer_points (
    customer_point_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    active_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    locked_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    lifetime_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    spent_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    expired_point NUMERIC(18, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_customer_points_customer
        FOREIGN KEY (customer_id) REFERENCES customer (customer_id) ON DELETE CASCADE,
    CONSTRAINT uq_customer_points_customer UNIQUE (customer_id),
    CONSTRAINT ck_customer_points_active CHECK (active_point >= 0),
    CONSTRAINT ck_customer_points_locked CHECK (locked_point >= 0),
    CONSTRAINT ck_customer_points_lifetime CHECK (lifetime_point >= 0),
    CONSTRAINT ck_customer_points_spent CHECK (spent_point >= 0),
    CONSTRAINT ck_customer_points_expired CHECK (expired_point >= 0)
);

CREATE TABLE voucher_definitions (
    voucher_definition_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(200),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    banner_image_url TEXT,
    reward_type VARCHAR(50) NOT NULL,
    reward_value NUMERIC(18, 2),
    validity_type VARCHAR(50) NOT NULL,
    valid_from TIMESTAMPTZ,
    valid_to TIMESTAMPTZ,
    duration_day INTEGER,
    generation_type VARCHAR(50) NOT NULL,
    publish_type VARCHAR(50) NOT NULL,
    total_stock INTEGER NOT NULL DEFAULT 0,
    remaining_stock INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ,

    CONSTRAINT uq_voucher_definitions_code UNIQUE (code),
    CONSTRAINT ck_voucher_definitions_total_stock CHECK (total_stock >= 0),
    CONSTRAINT ck_voucher_definitions_remaining_stock CHECK (
        remaining_stock >= 0 AND remaining_stock <= total_stock
    ),
    CONSTRAINT ck_voucher_definitions_duration_day CHECK (duration_day IS NULL OR duration_day > 0),
    CONSTRAINT ck_voucher_definitions_valid_range CHECK (
        valid_from IS NULL OR valid_to IS NULL OR valid_from < valid_to
    )
);

CREATE TABLE voucher_pools (
    voucher_pool_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    voucher_def_id UUID NOT NULL,
    voucher_code VARCHAR(200) NOT NULL,
    status VARCHAR(25) NOT NULL DEFAULT 'AVAILABLE',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_voucher_pools_definition
        FOREIGN KEY (voucher_def_id) REFERENCES voucher_definitions (voucher_definition_id) ON DELETE CASCADE,
    CONSTRAINT uq_voucher_pools_code UNIQUE (voucher_code),
    CONSTRAINT ck_voucher_pools_status CHECK (status IN ('AVAILABLE', 'CLAIMED', 'DISABLED'))
);

CREATE TABLE customer_vouchers (
    customer_voucher_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    voucher_def_id UUID NOT NULL,
    voucher_code VARCHAR(200) NOT NULL,
    voucher_pool_id UUID,
    valid_from TIMESTAMPTZ NOT NULL,
    valid_to TIMESTAMPTZ NOT NULL,
    remaining_count INTEGER NOT NULL DEFAULT 1,
    redeemed_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_customer_vouchers_customer
        FOREIGN KEY (customer_id) REFERENCES customer (customer_id),
    CONSTRAINT fk_customer_vouchers_definition
        FOREIGN KEY (voucher_def_id) REFERENCES voucher_definitions (voucher_definition_id),
    CONSTRAINT fk_customer_vouchers_pool
        FOREIGN KEY (voucher_pool_id) REFERENCES voucher_pools (voucher_pool_id),
    CONSTRAINT ck_customer_vouchers_valid_range CHECK (valid_from < valid_to),
    CONSTRAINT ck_customer_vouchers_remaining_count CHECK (remaining_count >= 0)
);

CREATE TABLE campaigns (
    campaign_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_name VARCHAR(200) NOT NULL,
    description TEXT,
    banner_image_url TEXT,
    event_type VARCHAR(50) NOT NULL,
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ NOT NULL,
    condition JSONB NOT NULL DEFAULT '{}',
    min_amount NUMERIC(18, 2),
    currency_code CHAR(3),
    schedule_cron VARCHAR(100),
    duration_hour INTEGER,
    user_limit_total INTEGER,
    user_limit_session INTEGER,
    status VARCHAR(25) NOT NULL DEFAULT 'DRAFT',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_campaigns_time_range CHECK (start_date < end_date),
    CONSTRAINT ck_campaigns_condition_object CHECK (jsonb_typeof(condition) = 'object'),
    CONSTRAINT ck_campaigns_min_amount CHECK (min_amount IS NULL OR min_amount >= 0),
    CONSTRAINT ck_campaigns_duration_hour CHECK (duration_hour IS NULL OR duration_hour > 0),
    CONSTRAINT ck_campaigns_user_limit_total CHECK (user_limit_total IS NULL OR user_limit_total > 0),
    CONSTRAINT ck_campaigns_user_limit_session CHECK (user_limit_session IS NULL OR user_limit_session > 0)
);

CREATE TABLE campaign_voucher_options (
    campaign_voucher_option_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_id UUID NOT NULL,
    voucher_definition_id UUID NOT NULL,
    point_cost NUMERIC(18, 2) NOT NULL DEFAULT 0,
    limit_per_customer INTEGER,
    display_order INTEGER NOT NULL DEFAULT 0,
    status VARCHAR(25) NOT NULL DEFAULT 'ACTIVE',
    available_from TIMESTAMPTZ,
    available_to TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_campaign_voucher_options_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (campaign_id) ON DELETE CASCADE,
    CONSTRAINT fk_campaign_voucher_options_definition
        FOREIGN KEY (voucher_definition_id) REFERENCES voucher_definitions (voucher_definition_id),
    CONSTRAINT uq_campaign_voucher_options_campaign_definition UNIQUE (campaign_id, voucher_definition_id),
    CONSTRAINT ck_campaign_voucher_options_point_cost CHECK (point_cost >= 0),
    CONSTRAINT ck_campaign_voucher_options_limit_per_customer CHECK (
        limit_per_customer IS NULL OR limit_per_customer > 0
    ),
    CONSTRAINT ck_campaign_voucher_options_display_order CHECK (display_order >= 0),
    CONSTRAINT ck_campaign_voucher_options_status CHECK (status IN ('ACTIVE', 'DISABLED')),
    CONSTRAINT ck_campaign_voucher_options_available_range CHECK (
        available_from IS NULL OR available_to IS NULL OR available_from < available_to
    )
);

CREATE TABLE campaign_sessions (
    campaign_session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_id UUID NOT NULL,
    session_start TIMESTAMPTZ NOT NULL,
    session_end TIMESTAMPTZ NOT NULL,
    status VARCHAR(25) NOT NULL DEFAULT 'SCHEDULED',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    ended_at TIMESTAMPTZ,

    CONSTRAINT fk_campaign_sessions_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (campaign_id) ON DELETE CASCADE,
    CONSTRAINT uq_campaign_sessions_campaign_start UNIQUE (campaign_id, session_start),
    CONSTRAINT ck_campaign_sessions_time_range CHECK (session_start < session_end),
    CONSTRAINT ck_campaign_sessions_status CHECK (status IN ('SCHEDULED', 'ON_GOING', 'ENDED', 'CANCELLED'))
);

CREATE TABLE actions (
    action_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reference_type VARCHAR(100) NOT NULL,
    reference_id UUID NOT NULL,
    action_type VARCHAR(50) NOT NULL,
    action_config JSONB NOT NULL DEFAULT '{}',
    execute_order INTEGER NOT NULL DEFAULT 0,
    total_count INTEGER,
    session_count INTEGER,
    used_count INTEGER NOT NULL DEFAULT 0,
    total_amount NUMERIC(18, 2),
    session_amount NUMERIC(18, 2),
    used_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_actions_config_object CHECK (jsonb_typeof(action_config) = 'object'),
    CONSTRAINT ck_actions_execute_order CHECK (execute_order >= 0),
    CONSTRAINT ck_actions_total_count CHECK (total_count IS NULL OR total_count >= 0),
    CONSTRAINT ck_actions_session_count CHECK (session_count IS NULL OR session_count >= 0),
    CONSTRAINT ck_actions_used_count CHECK (used_count >= 0),
    CONSTRAINT ck_actions_used_count_within_total CHECK (total_count IS NULL OR used_count <= total_count),
    CONSTRAINT ck_actions_total_amount CHECK (total_amount IS NULL OR total_amount >= 0),
    CONSTRAINT ck_actions_session_amount CHECK (session_amount IS NULL OR session_amount >= 0),
    CONSTRAINT ck_actions_used_amount CHECK (used_amount >= 0),
    CONSTRAINT ck_actions_used_amount_within_total CHECK (total_amount IS NULL OR used_amount <= total_amount)
);

CREATE TABLE campaign_usages (
    campaign_usage_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_id UUID NOT NULL,
    campaign_session_id UUID,
    customer_id UUID NOT NULL,
    action_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_campaign_usages_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (campaign_id),
    CONSTRAINT fk_campaign_usages_session
        FOREIGN KEY (campaign_session_id) REFERENCES campaign_sessions (campaign_session_id),
    CONSTRAINT fk_campaign_usages_customer
        FOREIGN KEY (customer_id) REFERENCES customer (customer_id),
    CONSTRAINT fk_campaign_usages_action
        FOREIGN KEY (action_id) REFERENCES actions (action_id)
);

CREATE TABLE action_usage (
    action_usage_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    action_id UUID NOT NULL,
    campaign_session_id UUID NOT NULL,
    used_count INTEGER NOT NULL DEFAULT 0,
    used_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_action_usage_action
        FOREIGN KEY (action_id) REFERENCES actions (action_id) ON DELETE CASCADE,
    CONSTRAINT fk_action_usage_session
        FOREIGN KEY (campaign_session_id) REFERENCES campaign_sessions (campaign_session_id) ON DELETE CASCADE,
    CONSTRAINT uq_action_usage_action_session UNIQUE (action_id, campaign_session_id),
    CONSTRAINT ck_action_usage_used_count CHECK (used_count >= 0),
    CONSTRAINT ck_action_usage_used_amount CHECK (used_amount >= 0)
);

CREATE TABLE point_transactions (
    point_transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    campaign_id UUID,
    campaign_session_id UUID,
    action_id UUID,
    source_event_id VARCHAR(100),
    transaction_type VARCHAR(50) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    balance_before NUMERIC(18, 2) NOT NULL,
    balance_after NUMERIC(18, 2) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_point_transactions_customer
        FOREIGN KEY (customer_id) REFERENCES customer (customer_id),
    CONSTRAINT fk_point_transactions_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (campaign_id),
    CONSTRAINT fk_point_transactions_session
        FOREIGN KEY (campaign_session_id) REFERENCES campaign_sessions (campaign_session_id),
    CONSTRAINT fk_point_transactions_action
        FOREIGN KEY (action_id) REFERENCES actions (action_id),
    CONSTRAINT ck_point_transactions_amount_not_zero CHECK (amount <> 0),
    CONSTRAINT ck_point_transactions_balance_before CHECK (balance_before >= 0),
    CONSTRAINT ck_point_transactions_balance_after CHECK (balance_after >= 0)
);

CREATE TABLE voucher_redemptions (
    voucher_redemption_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_voucher_id UUID NOT NULL,
    customer_id UUID NOT NULL,
    voucher_def_id UUID NOT NULL,
    voucher_pool_id UUID,
    campaign_id UUID,
    campaign_session_id UUID,
    action_id UUID,
    source_event_id VARCHAR(100),
    voucher_code VARCHAR(200) NOT NULL,
    redeemed_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_voucher_redemptions_customer_voucher
        FOREIGN KEY (customer_voucher_id) REFERENCES customer_vouchers (customer_voucher_id),
    CONSTRAINT fk_voucher_redemptions_customer
        FOREIGN KEY (customer_id) REFERENCES customer (customer_id),
    CONSTRAINT fk_voucher_redemptions_definition
        FOREIGN KEY (voucher_def_id) REFERENCES voucher_definitions (voucher_definition_id),
    CONSTRAINT fk_voucher_redemptions_pool
        FOREIGN KEY (voucher_pool_id) REFERENCES voucher_pools (voucher_pool_id),
    CONSTRAINT fk_voucher_redemptions_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (campaign_id),
    CONSTRAINT fk_voucher_redemptions_session
        FOREIGN KEY (campaign_session_id) REFERENCES campaign_sessions (campaign_session_id),
    CONSTRAINT fk_voucher_redemptions_action
        FOREIGN KEY (action_id) REFERENCES actions (action_id),
    CONSTRAINT uq_voucher_redemptions_customer_voucher UNIQUE (customer_voucher_id)
);

CREATE TABLE audit_logs (
    audit_log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    actor_user_id UUID,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID,
    old_value JSONB,
    new_value JSONB,
    metadata JSONB NOT NULL DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_audit_logs_actor
        FOREIGN KEY (actor_user_id) REFERENCES users (user_id),
    CONSTRAINT ck_audit_logs_metadata_object CHECK (jsonb_typeof(metadata) = 'object')
);

CREATE INDEX ix_audit_logs_created_at_id
    ON audit_logs (created_at DESC, audit_log_id DESC);

CREATE INDEX ix_audit_logs_entity_type_created_at_id
    ON audit_logs (entity_type, created_at DESC, audit_log_id DESC);

CREATE INDEX ix_audit_logs_action_created_at_id
    ON audit_logs (action, created_at DESC, audit_log_id DESC);
