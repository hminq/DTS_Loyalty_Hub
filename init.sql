/* CREATE DATABASE dts_loyal_hub_db; */

CREATE EXTENSION IF NOT EXISTS pgcrypto;

/* Enums */
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN
        CREATE TYPE user_role AS ENUM ('ADMIN', 'CUSTOMER');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'campaign_status') THEN
        CREATE TYPE campaign_status AS ENUM ('PREPARING', 'SCHEDULED', 'ACTIVE', 'DISABLED', 'ENDED');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'voucher_code_type') THEN
        CREATE TYPE voucher_code_type AS ENUM ('STATIC', 'DYNAMIC');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'campaign_reward_status') THEN
        CREATE TYPE campaign_reward_status AS ENUM ('ENABLED', 'DISABLED');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'voucher_code_generation_status') THEN
        CREATE TYPE voucher_code_generation_status AS ENUM (
            'NOT_REQUIRED',
            'PENDING',
            'GENERATING',
            'COMPLETED',
            'FAILED'
        );
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'discount_type') THEN
        CREATE TYPE discount_type AS ENUM ('PERCENT', 'FIXED_AMOUNT');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'point_transaction_type') THEN
        CREATE TYPE point_transaction_type AS ENUM ('INITIAL', 'REDEEM', 'REFUND', 'ADMIN_ADJUSTMENT');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'audit_entity_type') THEN
        CREATE TYPE audit_entity_type AS ENUM (
            'USER_TIER',
            'TIER_PERIOD',
            'POINT_WALLET',
            'CAMPAIGN',
            'CAMPAIGN_REWARD',
            'VOUCHER',
            'REDEMPTION_HISTORY',
            'POINT_TRANSACTION'
        );
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'audit_action_type') THEN
        CREATE TYPE audit_action_type AS ENUM (
            'CREATE',
            'UPDATE',
            'DELETE',
            'REDEEM',
            'AUTO_STATUS_CHANGE',
            'ADMIN_ADJUSTMENT'
        );
    END IF;
END $$ LANGUAGE plpgsql;

/* Tables */
CREATE TABLE IF NOT EXISTS user_tiers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL,
    sort_order INTEGER NOT NULL,
    min_points INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_user_tiers_name UNIQUE (name),
    CONSTRAINT uq_user_tiers_sort_order UNIQUE (sort_order),
    CONSTRAINT ck_user_tiers_sort_order_positive CHECK (sort_order > 0),
    CONSTRAINT ck_user_tiers_min_points_non_negative CHECK (min_points >= 0)
);

CREATE TABLE IF NOT EXISTS tier_periods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    start_at TIMESTAMPTZ NOT NULL,
    end_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_tier_periods_name UNIQUE (name),
    CONSTRAINT ck_tier_periods_time_range CHECK (start_at < end_at)
);

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tier_id UUID,
    username VARCHAR(100) NOT NULL,
    password_hash TEXT NOT NULL,
    role user_role NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_users_tier
        FOREIGN KEY (tier_id) REFERENCES user_tiers (id),
    CONSTRAINT uq_users_username UNIQUE (username)
);

CREATE TABLE IF NOT EXISTS refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token_hash TEXT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT uq_refresh_tokens_token_hash UNIQUE (token_hash)
);

CREATE TABLE IF NOT EXISTS point_wallets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    current_tier_period_id UUID,
    balance INTEGER NOT NULL DEFAULT 0,
    lifetime_points INTEGER NOT NULL DEFAULT 0,
    tier_points INTEGER NOT NULL DEFAULT 0,
    row_version BIGINT NOT NULL DEFAULT 0,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_point_wallets_user
        FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT fk_point_wallets_current_tier_period
        FOREIGN KEY (current_tier_period_id) REFERENCES tier_periods (id),
    CONSTRAINT uq_point_wallets_user UNIQUE (user_id),
    CONSTRAINT ck_point_wallets_balance_non_negative CHECK (balance >= 0),
    CONSTRAINT ck_point_wallets_lifetime_points_non_negative CHECK (lifetime_points >= 0),
    CONSTRAINT ck_point_wallets_tier_points_non_negative CHECK (tier_points >= 0)
);

CREATE TABLE IF NOT EXISTS point_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    wallet_id UUID NOT NULL,
    type point_transaction_type NOT NULL,
    amount INTEGER NOT NULL,
    balance_after INTEGER NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_point_transactions_wallet
        FOREIGN KEY (wallet_id) REFERENCES point_wallets (id),
    CONSTRAINT ck_point_transactions_amount_not_zero CHECK (amount <> 0),
    CONSTRAINT ck_point_transactions_balance_after_non_negative CHECK (balance_after >= 0)
);

CREATE TABLE IF NOT EXISTS campaigns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    status campaign_status NOT NULL DEFAULT 'PREPARING',
    created_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ,

    CONSTRAINT fk_campaigns_created_by
        FOREIGN KEY (created_by) REFERENCES users (id),
    CONSTRAINT ck_campaigns_time_range CHECK (start_time < end_time)
);

CREATE TABLE IF NOT EXISTS campaign_rewards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_id UUID NOT NULL,
    required_tier_id UUID,
    name VARCHAR(200) NOT NULL,
    status campaign_reward_status NOT NULL DEFAULT 'ENABLED',
    voucher_code_type voucher_code_type NOT NULL,
    code_generation_status voucher_code_generation_status NOT NULL DEFAULT 'NOT_REQUIRED',
    generated_code_count INTEGER NOT NULL DEFAULT 0,
    discount_type discount_type NOT NULL,
    discount_value NUMERIC(12, 2) NOT NULL DEFAULT 0,
    points_required INTEGER NOT NULL,
    total_stock INTEGER NOT NULL,
    available_stock INTEGER NOT NULL,
    row_version BIGINT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ,

    CONSTRAINT fk_campaign_rewards_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (id),
    CONSTRAINT fk_campaign_rewards_required_tier
        FOREIGN KEY (required_tier_id) REFERENCES user_tiers (id),
    CONSTRAINT ck_campaign_rewards_generated_code_count_non_negative CHECK (generated_code_count >= 0),
    CONSTRAINT ck_campaign_rewards_generated_code_count_within_total CHECK (generated_code_count <= total_stock),
    CONSTRAINT ck_campaign_rewards_points_positive CHECK (points_required > 0),
    CONSTRAINT ck_campaign_rewards_total_stock_non_negative CHECK (total_stock >= 0),
    CONSTRAINT ck_campaign_rewards_available_stock_non_negative CHECK (available_stock >= 0),
    CONSTRAINT ck_campaign_rewards_available_within_total CHECK (available_stock <= total_stock),
    CONSTRAINT ck_campaign_rewards_discount_non_negative CHECK (discount_value >= 0)
);

CREATE TABLE IF NOT EXISTS vouchers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_reward_id UUID NOT NULL,
    code VARCHAR(32) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    redeemed_at TIMESTAMPTZ,

    CONSTRAINT fk_vouchers_campaign_reward
        FOREIGN KEY (campaign_reward_id) REFERENCES campaign_rewards (id),
    CONSTRAINT uq_vouchers_id_campaign_reward UNIQUE (id, campaign_reward_id),
    CONSTRAINT uq_vouchers_code UNIQUE (code)
);

CREATE TABLE IF NOT EXISTS redemption_histories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    redeemed_by UUID NOT NULL,
    voucher_id UUID NOT NULL,
    campaign_reward_id UUID NOT NULL,
    redeemed_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_redemption_histories_user
        FOREIGN KEY (redeemed_by) REFERENCES users (id),
    CONSTRAINT fk_redemption_histories_voucher_reward
        FOREIGN KEY (voucher_id, campaign_reward_id) REFERENCES vouchers (id, campaign_reward_id),
    CONSTRAINT fk_redemption_histories_campaign_reward
        FOREIGN KEY (campaign_reward_id) REFERENCES campaign_rewards (id),
    CONSTRAINT uq_redemption_histories_user_reward UNIQUE (redeemed_by, campaign_reward_id)
);

CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    actor_id UUID,
    action audit_action_type NOT NULL,
    entity_type audit_entity_type NOT NULL,
    entity_id UUID,
    old_value JSONB,
    new_value JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_audit_logs_actor
        FOREIGN KEY (actor_id) REFERENCES users (id)
);
