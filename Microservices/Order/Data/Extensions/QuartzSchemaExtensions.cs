using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CryptoJackpot.Order.Data.Extensions;

/// <summary>
/// Extension methods for provisioning Quartz.NET database tables.
/// Ensures the required schema exists before Quartz AdoJobStore initializes.
/// Uses CREATE TABLE IF NOT EXISTS for idempotent execution.
/// </summary>
public static class QuartzSchemaExtensions
{
    /// <summary>
    /// Provisions Quartz.NET tables in the Order database.
    /// Call this AFTER EF migrations and BEFORE Quartz starts.
    /// Safe to call multiple times - all statements are idempotent.
    /// </summary>
    public static async Task ProvisionQuartzSchemaAsync<TContext>(
        this IHost host,
        int maxRetries = 3) where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (!await context.Database.CanConnectAsync())
                {
                    logger.LogWarning("[Quartz] Database not ready (attempt {Attempt}/{Max}). Waiting...",
                        attempt, maxRetries);
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                    continue;
                }

                await context.Database.ExecuteSqlRawAsync(QuartzPostgresTables);
                logger.LogInformation("[Quartz] Schema provisioned successfully in {Context}.", typeof(TContext).Name);
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                logger.LogInformation("[Quartz] Tables already exist. Skipping.");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "[Quartz] Provisioning failed (attempt {Attempt}/{Max}). Retrying...",
                    attempt, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
            }
        }

        logger.LogError("[Quartz] Failed to provision schema after {Max} attempts. Quartz may fail to start.", maxRetries);
    }

    /// <summary>
    /// Quartz.NET PostgreSQL schema in correct dependency order.
    /// All statements are idempotent (IF NOT EXISTS / ON CONFLICT DO NOTHING).
    /// </summary>
    public const string QuartzPostgresTables = """
        -- 1. Job Details (no dependencies)
        CREATE TABLE IF NOT EXISTS qrtz_job_details (
            sched_name VARCHAR(120) NOT NULL,
            job_name VARCHAR(200) NOT NULL,
            job_group VARCHAR(200) NOT NULL,
            description VARCHAR(250) NULL,
            job_class_name VARCHAR(250) NOT NULL,
            is_durable BOOLEAN NOT NULL,
            is_nonconcurrent BOOLEAN NOT NULL,
            is_update_data BOOLEAN NOT NULL,
            requests_recovery BOOLEAN NOT NULL,
            job_data BYTEA NULL,
            PRIMARY KEY (sched_name, job_name, job_group)
        );

        -- 2. Triggers (depends on job_details)
        CREATE TABLE IF NOT EXISTS qrtz_triggers (
            sched_name VARCHAR(120) NOT NULL,
            trigger_name VARCHAR(200) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            job_name VARCHAR(200) NOT NULL,
            job_group VARCHAR(200) NOT NULL,
            description VARCHAR(250) NULL,
            next_fire_time BIGINT NULL,
            prev_fire_time BIGINT NULL,
            priority INTEGER NULL,
            trigger_state VARCHAR(16) NOT NULL,
            trigger_type VARCHAR(8) NOT NULL,
            start_time BIGINT NOT NULL,
            end_time BIGINT NULL,
            calendar_name VARCHAR(200) NULL,
            misfire_instr SMALLINT NULL,
            job_data BYTEA NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, job_name, job_group)
                REFERENCES qrtz_job_details(sched_name, job_name, job_group)
        );

        -- 3. Simple Triggers
        CREATE TABLE IF NOT EXISTS qrtz_simple_triggers (
            sched_name VARCHAR(120) NOT NULL,
            trigger_name VARCHAR(200) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            repeat_count BIGINT NOT NULL,
            repeat_interval BIGINT NOT NULL,
            times_triggered BIGINT NOT NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE
        );

        -- 4. Cron Triggers
        CREATE TABLE IF NOT EXISTS qrtz_cron_triggers (
            sched_name VARCHAR(120) NOT NULL,
            trigger_name VARCHAR(200) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            cron_expression VARCHAR(120) NOT NULL,
            time_zone_id VARCHAR(80),
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE
        );

        -- 5. Simprop Triggers
        CREATE TABLE IF NOT EXISTS qrtz_simprop_triggers (
            sched_name VARCHAR(120) NOT NULL,
            trigger_name VARCHAR(200) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            str_prop_1 VARCHAR(512) NULL,
            str_prop_2 VARCHAR(512) NULL,
            str_prop_3 VARCHAR(512) NULL,
            int_prop_1 INTEGER NULL,
            int_prop_2 INTEGER NULL,
            long_prop_1 BIGINT NULL,
            long_prop_2 BIGINT NULL,
            dec_prop_1 NUMERIC(13,4) NULL,
            dec_prop_2 NUMERIC(13,4) NULL,
            bool_prop_1 BOOLEAN NULL,
            bool_prop_2 BOOLEAN NULL,
            time_zone_id VARCHAR(80) NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE
        );

        -- 6. Blob Triggers
        CREATE TABLE IF NOT EXISTS qrtz_blob_triggers (
            sched_name VARCHAR(120) NOT NULL,
            trigger_name VARCHAR(200) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            blob_data BYTEA NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE
        );

        -- 7. Calendars
        CREATE TABLE IF NOT EXISTS qrtz_calendars (
            sched_name VARCHAR(120) NOT NULL,
            calendar_name VARCHAR(200) NOT NULL,
            calendar BYTEA NOT NULL,
            PRIMARY KEY (sched_name, calendar_name)
        );

        -- 8. Paused Trigger Groups
        CREATE TABLE IF NOT EXISTS qrtz_paused_trigger_grps (
            sched_name VARCHAR(120) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            PRIMARY KEY (sched_name, trigger_group)
        );

        -- 9. Fired Triggers
        CREATE TABLE IF NOT EXISTS qrtz_fired_triggers (
            sched_name VARCHAR(120) NOT NULL,
            entry_id VARCHAR(140) NOT NULL,
            trigger_name VARCHAR(200) NOT NULL,
            trigger_group VARCHAR(200) NOT NULL,
            instance_name VARCHAR(200) NOT NULL,
            fired_time BIGINT NOT NULL,
            sched_time BIGINT NOT NULL,
            priority INTEGER NOT NULL,
            state VARCHAR(16) NOT NULL,
            job_name VARCHAR(200) NULL,
            job_group VARCHAR(200) NULL,
            is_nonconcurrent BOOLEAN NULL,
            requests_recovery BOOLEAN NULL,
            PRIMARY KEY (sched_name, entry_id)
        );

        -- 10. Scheduler State
        CREATE TABLE IF NOT EXISTS qrtz_scheduler_state (
            sched_name VARCHAR(120) NOT NULL,
            instance_name VARCHAR(200) NOT NULL,
            last_checkin_time BIGINT NOT NULL,
            checkin_interval BIGINT NOT NULL,
            PRIMARY KEY (sched_name, instance_name)
        );

        -- 11. Locks
        CREATE TABLE IF NOT EXISTS qrtz_locks (
            sched_name VARCHAR(120) NOT NULL,
            lock_name VARCHAR(40) NOT NULL,
            PRIMARY KEY (sched_name, lock_name)
        );

        -- Indexes
        CREATE INDEX IF NOT EXISTS idx_qrtz_j_req_recovery ON qrtz_job_details(sched_name, requests_recovery);
        CREATE INDEX IF NOT EXISTS idx_qrtz_j_grp ON qrtz_job_details(sched_name, job_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_j ON qrtz_triggers(sched_name, job_name, job_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_jg ON qrtz_triggers(sched_name, job_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_c ON qrtz_triggers(sched_name, calendar_name);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_g ON qrtz_triggers(sched_name, trigger_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_state ON qrtz_triggers(sched_name, trigger_state);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_n_state ON qrtz_triggers(sched_name, trigger_name, trigger_group, trigger_state);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_n_g_state ON qrtz_triggers(sched_name, trigger_group, trigger_state);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_next_fire_time ON qrtz_triggers(sched_name, next_fire_time);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_nft_st ON qrtz_triggers(sched_name, trigger_state, next_fire_time);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_nft_misfire ON qrtz_triggers(sched_name, misfire_instr, next_fire_time);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_nft_st_misfire ON qrtz_triggers(sched_name, misfire_instr, next_fire_time, trigger_state);
        CREATE INDEX IF NOT EXISTS idx_qrtz_t_nft_st_misfire_grp ON qrtz_triggers(sched_name, misfire_instr, next_fire_time, trigger_group, trigger_state);
        CREATE INDEX IF NOT EXISTS idx_qrtz_ft_trig_inst_name ON qrtz_fired_triggers(sched_name, instance_name);
        CREATE INDEX IF NOT EXISTS idx_qrtz_ft_inst_job_req_rcvry ON qrtz_fired_triggers(sched_name, instance_name, requests_recovery);
        CREATE INDEX IF NOT EXISTS idx_qrtz_ft_j_g ON qrtz_fired_triggers(sched_name, job_name, job_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_ft_jg ON qrtz_fired_triggers(sched_name, job_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_ft_t_g ON qrtz_fired_triggers(sched_name, trigger_name, trigger_group);
        CREATE INDEX IF NOT EXISTS idx_qrtz_ft_tg ON qrtz_fired_triggers(sched_name, trigger_group);

        -- Initialize locks (required for Quartz clustering)
        INSERT INTO qrtz_locks (sched_name, lock_name)
        VALUES ('OrderTimeoutScheduler', 'TRIGGER_ACCESS')
        ON CONFLICT (sched_name, lock_name) DO NOTHING;

        INSERT INTO qrtz_locks (sched_name, lock_name)
        VALUES ('OrderTimeoutScheduler', 'STATE_ACCESS')
        ON CONFLICT (sched_name, lock_name) DO NOTHING;
        """;
}