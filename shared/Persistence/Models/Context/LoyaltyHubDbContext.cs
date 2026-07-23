using Microsoft.EntityFrameworkCore;

namespace Persistence.Models.Context;

public partial class LoyaltyHubDbContext : DbContext
{
    public LoyaltyHubDbContext(DbContextOptions<LoyaltyHubDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<ActionUsage> ActionUsages { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AdminSession> AdminSessions { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<CampaignSession> CampaignSessions { get; set; }

    public virtual DbSet<CampaignUsage> CampaignUsages { get; set; }


    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerPoint> CustomerPoints { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PointTransaction> PointTransactions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<TiersConfig> TiersConfigs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VoucherDefinition> VoucherDefinitions { get; set; }

    public virtual DbSet<VoucherPool> VoucherPools { get; set; }

    public virtual DbSet<VoucherRedemption> VoucherRedemptions { get; set; }

    public virtual DbSet<NotificationEventType> NotificationEventTypes { get; set; }

    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }

    public virtual DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("actions_pkey");

            entity.ToTable("actions");

            entity.Property(e => e.ActionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("action_id");
            entity.Property(e => e.ActionConfig)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("action_config");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .HasColumnName("action_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExecuteOrder).HasColumnName("execute_order");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(100)
                .HasColumnName("reference_type");
            entity.Property(e => e.SessionAmount)
                .HasPrecision(18, 2)
                .HasColumnName("session_amount");
            entity.Property(e => e.SessionCount).HasColumnName("session_count");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalCount).HasColumnName("total_count");
            entity.Property(e => e.UsedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("used_amount");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");
        });

        modelBuilder.Entity<ActionUsage>(entity =>
        {
            entity.HasKey(e => e.ActionUsageId).HasName("action_usage_pkey");

            entity.ToTable("action_usage");

            entity.HasIndex(e => new { e.ActionId, e.CampaignSessionId }, "uq_action_usage_action_session").IsUnique();

            entity.Property(e => e.ActionUsageId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("action_usage_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.CampaignSessionId).HasColumnName("campaign_session_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UsedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("used_amount");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");

            entity.HasOne(d => d.Action).WithMany(p => p.ActionUsages)
                .HasForeignKey(d => d.ActionId)
                .HasConstraintName("fk_action_usage_action");

            entity.HasOne(d => d.CampaignSession).WithMany(p => p.ActionUsages)
                .HasForeignKey(d => d.CampaignSessionId)
                .HasConstraintName("fk_action_usage_session");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("admin_pkey");

            entity.ToTable("admin");

            entity.HasIndex(e => e.UserId, "uq_admin_user").IsUnique();

            entity.Property(e => e.AdminId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Admins)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_admin_role");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .HasConstraintName("fk_admin_user");
        });

        modelBuilder.Entity<AdminSession>(entity =>
        {
            entity.HasKey(e => e.AdminSessionId).HasName("admin_sessions_pkey");

            entity.ToTable("admin_sessions");

            entity.HasIndex(e => new { e.AdminSessionId, e.AccessTokenJti, e.AdminId }, "ix_admin_sessions_session_jti_admin");

            entity.HasIndex(e => e.UserId, "ix_admin_sessions_user");

            entity.HasIndex(e => e.AccessTokenJti, "uq_admin_sessions_access_token_jti").IsUnique();

            entity.HasIndex(e => e.AdminId, "uq_admin_sessions_one_active_per_admin")
                .IsUnique()
                .HasFilter("(revoked_at IS NULL)");

            entity.Property(e => e.AdminSessionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("admin_session_id");
            entity.Property(e => e.AccessTokenJti).HasColumnName("access_token_jti");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Admin).WithMany()
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("fk_admin_sessions_admin");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_admin_sessions_user");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs");

            entity.Property(e => e.AuditLogId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("audit_log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .HasColumnName("entity_type");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.NewValue)
                .HasColumnType("jsonb")
                .HasColumnName("new_value");
            entity.Property(e => e.OldValue)
                .HasColumnType("jsonb")
                .HasColumnName("old_value");

            entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActorUserId)
                .HasConstraintName("fk_audit_logs_actor");
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("campaigns_pkey");

            entity.ToTable("campaigns");

            entity.Property(e => e.CampaignId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("campaign_id");
            entity.Property(e => e.BannerImageUrl).HasColumnName("banner_image_url");
            entity.Property(e => e.CampaignName)
                .HasMaxLength(200)
                .HasColumnName("campaign_name");
            entity.Property(e => e.Condition)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("condition");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currency_code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationHour).HasColumnName("duration_hour");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.MinAmount)
                .HasPrecision(18, 2)
                .HasColumnName("min_amount");
            entity.Property(e => e.ScheduleCron)
                .HasMaxLength(100)
                .HasColumnName("schedule_cron");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserLimitSession).HasColumnName("user_limit_session");
            entity.Property(e => e.UserLimitTotal).HasColumnName("user_limit_total");
        });

        modelBuilder.Entity<CampaignSession>(entity =>
        {
            entity.HasKey(e => e.CampaignSessionId).HasName("campaign_sessions_pkey");

            entity.ToTable("campaign_sessions");

            entity.HasIndex(e => new { e.CampaignId, e.SessionStart }, "uq_campaign_sessions_campaign_start").IsUnique();

            entity.Property(e => e.CampaignSessionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("campaign_session_id");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndedAt).HasColumnName("ended_at");
            entity.Property(e => e.SessionEnd).HasColumnName("session_end");
            entity.Property(e => e.SessionStart).HasColumnName("session_start");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValueSql("'SCHEDULED'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignSessions)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("fk_campaign_sessions_campaign");
        });

        modelBuilder.Entity<CampaignUsage>(entity =>
        {
            entity.HasKey(e => e.CampaignUsageId).HasName("campaign_usages_pkey");

            entity.ToTable("campaign_usages");

            entity.Property(e => e.CampaignUsageId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("campaign_usage_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.CampaignSessionId).HasColumnName("campaign_session_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");

            entity.HasOne(d => d.Action).WithMany(p => p.CampaignUsages)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_campaign_usages_action");

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignUsages)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_campaign_usages_campaign");

            entity.HasOne(d => d.CampaignSession).WithMany(p => p.CampaignUsages)
                .HasForeignKey(d => d.CampaignSessionId)
                .HasConstraintName("fk_campaign_usages_session");

            entity.HasOne(d => d.Customer).WithMany(p => p.CampaignUsages)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_campaign_usages_customer");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customer_pkey");

            entity.ToTable("customer");

            entity.HasIndex(e => e.UserId, "uq_customer_user").IsUnique();

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentTierPoint)
                .HasPrecision(18, 2)
                .HasColumnName("current_tier_point");
            entity.Property(e => e.NextTierPoint)
                .HasPrecision(18, 2)
                .HasColumnName("next_tier_point");
            entity.Property(e => e.StartTier)
                 .HasColumnName("start_tier");

            entity.Property(e => e.ExpiredTier)
                .HasColumnName("expired_tier");

            entity.Property(e => e.NextTierId)
                .HasColumnName("next_tier_id");
            entity.Property(e => e.TierId).HasColumnName("tier_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Tier).WithMany(p => p.Customers)
                .HasForeignKey(d => d.TierId)
                .HasConstraintName("fk_customer_tier");
            entity.HasOne(d => d.NextTier)
                .WithMany(p => p.NextTierCustomers)
                .HasForeignKey(d => d.NextTierId)
                .HasConstraintName("fk_customer_next_tier");
            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .HasConstraintName("fk_customer_user");
        });

        modelBuilder.Entity<CustomerPoint>(entity =>
        {
            entity.HasKey(e => e.CustomerPointId).HasName("customer_points_pkey");

            entity.ToTable("customer_points");

            entity.HasIndex(e => e.CustomerId, "uq_customer_points_customer").IsUnique();

            entity.Property(e => e.CustomerPointId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_point_id");
            entity.Property(e => e.ActivePoint)
                .HasPrecision(18, 2)
                .HasColumnName("active_point");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ExpiredPoint)
                .HasPrecision(18, 2)
                .HasColumnName("expired_point");
            entity.Property(e => e.LifetimePoint)
                .HasPrecision(18, 2)
                .HasColumnName("lifetime_point");
            entity.Property(e => e.LockedPoint)
                .HasPrecision(18, 2)
                .HasColumnName("locked_point");
            entity.Property(e => e.SpentPoint)
                .HasPrecision(18, 2)
                .HasColumnName("spent_point");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithOne(p => p.CustomerPoint)
                .HasForeignKey<CustomerPoint>(d => d.CustomerId)
                .HasConstraintName("fk_customer_points_customer");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => e.CustomerVoucherId).HasName("customer_vouchers_pkey");

            entity.ToTable("customer_vouchers");

            entity.Property(e => e.CustomerVoucherId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_voucher_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.RedeemedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("redeemed_at");
            entity.Property(e => e.RemainingCount)
                .HasDefaultValue(1)
                .HasColumnName("remaining_count");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.VoucherCode)
                .HasMaxLength(200)
                .HasColumnName("voucher_code");
            entity.Property(e => e.VoucherDefId).HasColumnName("voucher_def_id");
            entity.Property(e => e.VoucherPoolId).HasColumnName("voucher_pool_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_vouchers_customer");

            entity.HasOne(d => d.VoucherDef).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherDefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_vouchers_definition");

            entity.HasOne(d => d.VoucherPool).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherPoolId)
                .HasConstraintName("fk_customer_vouchers_pool");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("permissions_pkey");

            entity.ToTable("permissions", table => table.HasCheckConstraint(
                "ck_permissions_code_group_action",
                "code = group_code || '.' || action_code"));

            entity.HasIndex(e => e.Code, "uq_permissions_code").IsUnique();
            entity.HasIndex(e => new { e.GroupCode, e.ActionCode }, "uq_permissions_group_action").IsUnique();

            entity.Property(e => e.PermissionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("permission_id");
            entity.Property(e => e.ActionCode)
                .HasMaxLength(50)
                .HasColumnName("action_code");
            entity.Property(e => e.ActionName)
                .HasMaxLength(100)
                .HasColumnName("action_name");
            entity.Property(e => e.ActionSortOrder).HasColumnName("action_sort_order");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupCode)
                .HasMaxLength(50)
                .HasColumnName("group_code");
            entity.Property(e => e.GroupName)
                .HasMaxLength(100)
                .HasColumnName("group_name");
            entity.Property(e => e.GroupSortOrder).HasColumnName("group_sort_order");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PointTransaction>(entity =>
        {
            entity.HasKey(e => e.PointTransactionId).HasName("point_transactions_pkey");

            entity.ToTable("point_transactions");

            entity.Property(e => e.PointTransactionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("point_transaction_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BalanceAfter)
                .HasPrecision(18, 2)
                .HasColumnName("balance_after");
            entity.Property(e => e.BalanceBefore)
                .HasPrecision(18, 2)
                .HasColumnName("balance_before");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.CampaignSessionId).HasColumnName("campaign_session_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.SourceEventId)
                .HasMaxLength(100)
                .HasColumnName("source_event_id");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Action).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.ActionId)
                .HasConstraintName("fk_point_transactions_action");

            entity.HasOne(d => d.Campaign).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("fk_point_transactions_campaign");

            entity.HasOne(d => d.CampaignSession).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.CampaignSessionId)
                .HasConstraintName("fk_point_transactions_session");

            entity.HasOne(d => d.Customer).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_point_transactions_customer");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "uq_roles_name").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("role_permissions_pkey");

            entity.ToTable("role_permissions");

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "uq_role_permissions_role_permission").IsUnique();

            entity.Property(e => e.RolePermissionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("role_permission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("fk_role_permissions_permission");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_role_permissions_role");
        });

        modelBuilder.Entity<TiersConfig>(entity =>
        {
            entity.HasKey(e => e.TierConfigId).HasName("tiers_config_pkey");

            entity.ToTable("tiers_config");

            entity.HasIndex(e => e.Name, "uq_tiers_config_name").IsUnique();

            entity.HasIndex(e => e.Priority, "uq_tiers_config_priority").IsUnique();

            entity.Property(e => e.TierConfigId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("tier_config_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CycleMonth)
                .HasDefaultValue(3)
                .HasColumnName("cycle_month");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.PointsRequired)
                .HasPrecision(18, 2)
                .HasColumnName("points_required");
            entity.Property(e => e.Priority).HasColumnName("priority");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "uq_users_phone_number").IsUnique();

            entity.HasIndex(e => e.Username, "uq_users_username").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValueSql("'ENABLE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserType)
                .HasMaxLength(25)
                .HasColumnName("user_type");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<VoucherDefinition>(entity =>
        {
            entity.HasKey(e => e.VoucherDefinitionId).HasName("voucher_definitions_pkey");

            entity.ToTable("voucher_definitions");

            entity.HasIndex(e => e.Code, "uq_voucher_definitions_code").IsUnique();

            entity.Property(e => e.VoucherDefinitionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("voucher_definition_id");
            entity.Property(e => e.BannerImageUrl).HasColumnName("banner_image_url");
            entity.Property(e => e.Code)
                .HasMaxLength(200)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationDay).HasColumnName("duration_day");
            entity.Property(e => e.GenerationType)
                .HasMaxLength(50)
                .HasColumnName("generation_type");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.PublishType)
                .HasMaxLength(50)
                .HasColumnName("publish_type");
            entity.Property(e => e.RewardType)
                .HasMaxLength(50)
                .HasColumnName("reward_type");
            entity.Property(e => e.RewardValue)
                .HasPrecision(18, 2)
                .HasColumnName("reward_value");
            entity.Property(e => e.RemainingStock)
                .HasDefaultValue(0)
                .HasColumnName("remaining_stock");
            entity.Property(e => e.TotalStock).HasColumnName("total_stock");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.ValidityType)
                .HasMaxLength(50)
                .HasColumnName("validity_type");
        });

        modelBuilder.Entity<VoucherPool>(entity =>
        {
            entity.HasKey(e => e.VoucherPoolId).HasName("voucher_pools_pkey");

            entity.ToTable("voucher_pools");

            entity.HasIndex(e => e.VoucherCode, "uq_voucher_pools_code").IsUnique();

            entity.Property(e => e.VoucherPoolId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("voucher_pool_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValueSql("'AVAILABLE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.VoucherCode)
                .HasMaxLength(200)
                .HasColumnName("voucher_code");
            entity.Property(e => e.VoucherDefId).HasColumnName("voucher_def_id");

            entity.HasOne(d => d.VoucherDef).WithMany(p => p.VoucherPools)
                .HasForeignKey(d => d.VoucherDefId)
                .HasConstraintName("fk_voucher_pools_definition");
        });

        modelBuilder.Entity<VoucherRedemption>(entity =>
        {
            entity.HasKey(e => e.VoucherRedemptionId).HasName("voucher_redemptions_pkey");

            entity.ToTable("voucher_redemptions");

            entity.HasIndex(e => e.CustomerVoucherId, "uq_voucher_redemptions_customer_voucher").IsUnique();

            entity.Property(e => e.VoucherRedemptionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("voucher_redemption_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.CampaignSessionId).HasColumnName("campaign_session_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerVoucherId).HasColumnName("customer_voucher_id");
            entity.Property(e => e.RedeemedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("redeemed_at");
            entity.Property(e => e.SourceEventId)
                .HasMaxLength(100)
                .HasColumnName("source_event_id");
            entity.Property(e => e.VoucherCode)
                .HasMaxLength(200)
                .HasColumnName("voucher_code");
            entity.Property(e => e.VoucherDefId).HasColumnName("voucher_def_id");
            entity.Property(e => e.VoucherPoolId).HasColumnName("voucher_pool_id");

            entity.HasOne(d => d.Action).WithMany(p => p.VoucherRedemptions)
                .HasForeignKey(d => d.ActionId)
                .HasConstraintName("fk_voucher_redemptions_action");

            entity.HasOne(d => d.Campaign).WithMany(p => p.VoucherRedemptions)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("fk_voucher_redemptions_campaign");

            entity.HasOne(d => d.CampaignSession).WithMany(p => p.VoucherRedemptions)
                .HasForeignKey(d => d.CampaignSessionId)
                .HasConstraintName("fk_voucher_redemptions_session");

            entity.HasOne(d => d.Customer).WithMany(p => p.VoucherRedemptions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_voucher_redemptions_customer");

            entity.HasOne(d => d.CustomerVoucher).WithOne(p => p.VoucherRedemption)
                .HasForeignKey<VoucherRedemption>(d => d.CustomerVoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_voucher_redemptions_customer_voucher");

            entity.HasOne(d => d.VoucherDef).WithMany(p => p.VoucherRedemptions)
                .HasForeignKey(d => d.VoucherDefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_voucher_redemptions_definition");

            entity.HasOne(d => d.VoucherPool).WithMany(p => p.VoucherRedemptions)
                .HasForeignKey(d => d.VoucherPoolId)
                .HasConstraintName("fk_voucher_redemptions_pool");
        });

        modelBuilder.Entity<NotificationEventType>(entity =>
        {
            entity.HasKey(e => e.NotificationEventTypeId).HasName("notification_event_type_pkey");
            entity.ToTable("notification_event_type");
            entity.HasIndex(e => e.EventTypeCode, "uq_notification_event_type_code").IsUnique();

            entity.Property(e => e.NotificationEventTypeId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("notification_event_type_id");
            entity.Property(e => e.EventTypeCode)
                .HasMaxLength(100)
                .HasColumnName("event_type_code");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(255)
                .HasColumnName("display_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.AvailableVariables)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("available_variables");
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("notification_template_pkey");
            entity.ToTable("notification_template");
            entity.HasIndex(e => new { e.NotificationEventTypeId, e.Channel, e.Language, e.IsActive }, "idx_notif_template_lookup");

            entity.Property(e => e.TemplateId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("template_id");
            entity.Property(e => e.NotificationEventTypeId).HasColumnName("notification_event_type_id");
            entity.Property(e => e.Channel)
                .HasMaxLength(50)
                .HasDefaultValueSql("'PUSH'::character varying")
                .HasColumnName("channel");
            entity.Property(e => e.Language)
                .HasMaxLength(10)
                .HasDefaultValueSql("'vi'::character varying")
                .HasColumnName("language");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.TitleTemplate).HasColumnName("title_template");
            entity.Property(e => e.BodyTemplate).HasColumnName("body_template");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.NotificationEventType).WithMany(p => p.NotificationTemplates)
                .HasForeignKey(d => d.NotificationEventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notification_template_event_type");
        });

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("notification_log_pkey");
            entity.ToTable("notification_log");
            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt }, "idx_notif_log_customer");

            entity.Property(e => e.LogId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("log_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.EventTypeCode)
                .HasMaxLength(100)
                .HasColumnName("event_type_code");
            entity.Property(e => e.Channel)
                .HasMaxLength(50)
                .HasColumnName("channel");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.RenderedTitle).HasColumnName("rendered_title");
            entity.Property(e => e.RenderedBody).HasColumnName("rendered_body");
            entity.Property(e => e.VariablesSnapshot)
                .HasColumnType("jsonb")
                .HasColumnName("variables_snapshot");
            entity.Property(e => e.SourceRefType)
                .HasMaxLength(100)
                .HasColumnName("source_ref_type");
            entity.Property(e => e.SourceRefId).HasColumnName("source_ref_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.NotificationLogs)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notification_log_customer");

            entity.HasOne(d => d.Template).WithMany(p => p.NotificationLogs)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("fk_notification_log_template");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
