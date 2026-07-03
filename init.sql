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

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'discount_type') THEN
        CREATE TYPE discount_type AS ENUM ('PERCENT', 'FIXED_AMOUNT');
    END IF;
END $$ LANGUAGE plpgsql;

/* Tables */
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(100) NOT NULL,
    password_hash TEXT NOT NULL,
    role user_role NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_users_username UNIQUE (username)
);

CREATE TABLE IF NOT EXISTS point_wallets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    balance INTEGER NOT NULL DEFAULT 0,
    row_version BIGINT NOT NULL DEFAULT 0,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_point_wallets_user
        FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT uq_point_wallets_user UNIQUE (user_id),
    CONSTRAINT ck_point_wallets_balance_non_negative CHECK (balance >= 0)
);

CREATE TABLE IF NOT EXISTS campaigns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    status campaign_status NOT NULL DEFAULT 'PREPARING',
    created_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_campaigns_created_by
        FOREIGN KEY (created_by) REFERENCES users (id),
    CONSTRAINT ck_campaigns_time_range CHECK (start_time < end_time)
);

CREATE TABLE IF NOT EXISTS campaign_rewards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    voucher_code_type voucher_code_type NOT NULL,
    discount_type discount_type NOT NULL,
    discount_value NUMERIC(12, 2) NOT NULL DEFAULT 0,
    points_required INTEGER NOT NULL,
    total_stock INTEGER NOT NULL,
    available_stock INTEGER NOT NULL,
    row_version BIGINT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_campaign_rewards_campaign
        FOREIGN KEY (campaign_id) REFERENCES campaigns (id),
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

    CONSTRAINT fk_vouchers_campaign_reward
        FOREIGN KEY (campaign_reward_id) REFERENCES campaign_rewards (id),
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
    CONSTRAINT fk_redemption_histories_voucher
        FOREIGN KEY (voucher_id) REFERENCES vouchers (id),
    CONSTRAINT fk_redemption_histories_campaign_reward
        FOREIGN KEY (campaign_reward_id) REFERENCES campaign_rewards (id),
    CONSTRAINT uq_redemption_histories_user_reward UNIQUE (redeemed_by, campaign_reward_id)
);

CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entity_name VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    old_value JSONB,
    new_value JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
