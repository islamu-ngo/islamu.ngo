// ABOUTME: Removes the retired Event/EventSession product schema while retaining generic platform chassis tables.
// ABOUTME: Rebuilds the retired schema explicitly in Down without replaying migrations or restoring product data.

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Explore.Persistence.Migrations;

public partial class RemoveEventProductSurface : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        RemoveEventImageStoragePurpose(migrationBuilder);

        migrationBuilder.Sql("DROP VIEW IF EXISTS vw_event_with_sessions;");

        migrationBuilder.DropForeignKey("fk_event_custom_property_definitions_event_custom_property_opt", "event_custom_property_definitions");
        migrationBuilder.DropForeignKey("fk_event_session_custom_property_definitions_event_session_cus", "event_session_custom_property_definitions");
        migrationBuilder.DropForeignKey("fk_event_session_template_custom_property_definitions_event_se", "event_session_template_custom_property_definitions");
        migrationBuilder.DropForeignKey("fk_event_template_custom_property_definitions_event_template_c", "event_template_custom_property_definitions");
        migrationBuilder.DropForeignKey("fk_email_dispatch_outbox_events_event_id", "email_dispatch_outbox");
        migrationBuilder.DropForeignKey("fk_email_dispatch_outbox_event_registration_intents_registrati", "email_dispatch_outbox");
        migrationBuilder.DropForeignKey("fk_integration_sync_outbox_events_event_id", "integration_sync_outbox");
        migrationBuilder.DropForeignKey("fk_integration_sync_outbox_event_registration_intents_registra", "integration_sync_outbox");
        migrationBuilder.DropForeignKey("fk_notification_intents_events_event_id", "notification_intents");
        migrationBuilder.DropForeignKey("fk_notification_intents_event_reports_report_id", "notification_intents");
        migrationBuilder.DropForeignKey("fk_notification_intents_event_report_decisions_report_decision", "notification_intents");
        migrationBuilder.DropForeignKey("fk_notification_external_delegations_event_reports_report_id", "notification_external_delegations");
        migrationBuilder.DropForeignKey("fk_notification_external_delegations_event_report_decisions_re", "notification_external_delegations");
        migrationBuilder.DropForeignKey("fk_organization_reviews_events_event_id", "organization_reviews");

        migrationBuilder.DropIndex("ix_email_dispatch_outbox_event_id", "email_dispatch_outbox");
        migrationBuilder.DropIndex("ix_email_dispatch_outbox_registration_intent_id", "email_dispatch_outbox");
        migrationBuilder.DropIndex("ix_integration_sync_outbox_event_id", "integration_sync_outbox");
        migrationBuilder.DropIndex("ix_integration_sync_outbox_registration_intent_id", "integration_sync_outbox");
        migrationBuilder.DropIndex("ix_notification_intents_event_id", "notification_intents");
        migrationBuilder.DropIndex("ix_notification_intents_report_id", "notification_intents");
        migrationBuilder.DropIndex("ix_notification_intents_report_decision_id", "notification_intents");
        migrationBuilder.DropIndex("ix_notification_external_delegations_report_id", "notification_external_delegations");
        migrationBuilder.DropIndex("ix_notification_external_delegations_report_decision_id", "notification_external_delegations");
        migrationBuilder.DropIndex("ix_organization_reviews_event_id", "organization_reviews");

        migrationBuilder.DropColumn("event_id", "email_dispatch_outbox");
        migrationBuilder.DropColumn("registration_intent_id", "email_dispatch_outbox");
        migrationBuilder.DropColumn("event_id", "integration_sync_outbox");
        migrationBuilder.DropColumn("registration_intent_id", "integration_sync_outbox");
        migrationBuilder.DropColumn("event_id", "notification_intents");
        migrationBuilder.DropColumn("report_id", "notification_intents");
        migrationBuilder.DropColumn("report_decision_id", "notification_intents");
        migrationBuilder.DropColumn("report_id", "notification_external_delegations");
        migrationBuilder.DropColumn("report_decision_id", "notification_external_delegations");
        migrationBuilder.DropColumn("event_id", "organization_reviews");

        migrationBuilder.DropColumn("events_allow_user_submitted_events_local_value", "instance_policy_sets");
        migrationBuilder.DropColumn("events_allow_user_submitted_events_override_mode", "instance_policy_sets");
        migrationBuilder.DropColumn("events_allow_organization_submitted_events_local_value", "instance_policy_sets");
        migrationBuilder.DropColumn("events_allow_organization_submitted_events_override_mode", "instance_policy_sets");
        migrationBuilder.DropColumn("events_allow_group_submitted_events_local_value", "instance_policy_sets");
        migrationBuilder.DropColumn("events_allow_group_submitted_events_override_mode", "instance_policy_sets");
        migrationBuilder.DropColumn("events_event_card_click_opens_detail_page_local_value", "instance_policy_sets");
        migrationBuilder.DropColumn("events_event_card_click_opens_detail_page_override_mode", "instance_policy_sets");
        migrationBuilder.DropColumn("modules_enable_islamic_module_local_value", "instance_policy_sets");
        migrationBuilder.DropColumn("modules_enable_islamic_module_override_mode", "instance_policy_sets");
        migrationBuilder.DropColumn("modules_enable_tech_module_local_value", "instance_policy_sets");
        migrationBuilder.DropColumn("modules_enable_tech_module_override_mode", "instance_policy_sets");

        migrationBuilder.DropColumn("events_allow_user_submitted_events_local_value", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_allow_user_submitted_events_override_mode", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_allow_organization_submitted_events_local_value", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_allow_organization_submitted_events_override_mode", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_allow_group_submitted_events_local_value", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_allow_group_submitted_events_override_mode", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_event_card_click_opens_detail_page_local_value", "tenant_policy_sets");
        migrationBuilder.DropColumn("events_event_card_click_opens_detail_page_override_mode", "tenant_policy_sets");

        migrationBuilder.DropColumn("events_allow_user_submitted_events_local_value", "organization_policy_sets");
        migrationBuilder.DropColumn("events_allow_user_submitted_events_override_mode", "organization_policy_sets");
        migrationBuilder.DropColumn("events_allow_organization_submitted_events_local_value", "organization_policy_sets");
        migrationBuilder.DropColumn("events_allow_organization_submitted_events_override_mode", "organization_policy_sets");
        migrationBuilder.DropColumn("events_allow_group_submitted_events_local_value", "organization_policy_sets");
        migrationBuilder.DropColumn("events_allow_group_submitted_events_override_mode", "organization_policy_sets");
        migrationBuilder.DropColumn("events_event_card_click_opens_detail_page_local_value", "organization_policy_sets");
        migrationBuilder.DropColumn("events_event_card_click_opens_detail_page_override_mode", "organization_policy_sets");

        migrationBuilder.DropTable("custom_property_projection_dirty_scope");
        migrationBuilder.DropTable("custom_property_projection_status");

        migrationBuilder.DropTable("event_contact_share_export_items");
        migrationBuilder.DropTable("event_contact_share_exports");
        migrationBuilder.DropTable("event_contact_share_consents");
        migrationBuilder.DropTable("event_session_group_sessions");
        migrationBuilder.DropTable("event_session_categories");
        migrationBuilder.DropTable("event_session_tags");
        migrationBuilder.DropTable("event_session_languages");
        migrationBuilder.DropTable("event_session_speakers");
        migrationBuilder.DropTable("event_session_agenda_items");
        migrationBuilder.DropTable("event_session_custom_property_projections");
        migrationBuilder.DropTable("event_session_custom_property_values");
        migrationBuilder.DropTable("event_session_custom_property_options");
        migrationBuilder.DropTable("event_session_custom_property_definitions");
        migrationBuilder.DropTable("event_session_islamic_aspects");
        migrationBuilder.DropTable("event_registrations");
        migrationBuilder.DropTable("event_registration_intents");
        migrationBuilder.DropTable("event_report_external_links");
        migrationBuilder.DropTable("event_report_signals");
        migrationBuilder.DropTable("event_moderation_records");
        migrationBuilder.DropTable("event_report_decisions");
        migrationBuilder.DropTable("event_report_evidence");
        migrationBuilder.DropTable("event_report_targets");
        migrationBuilder.DropTable("event_report_cases");
        migrationBuilder.DropTable("event_reports");
        migrationBuilder.DropTable("event_custom_property_projections");
        migrationBuilder.DropTable("event_custom_property_values");
        migrationBuilder.DropTable("event_custom_property_options");
        migrationBuilder.DropTable("event_custom_property_definitions");
        migrationBuilder.DropTable("event_categories");
        migrationBuilder.DropTable("event_tags");
        migrationBuilder.DropTable("event_agenda_items");
        migrationBuilder.DropTable("event_sessions");
        migrationBuilder.DropTable("event_session_groups");
        migrationBuilder.DropTable("event_days");
        migrationBuilder.DropTable("event_role_assignments");
        migrationBuilder.DropTable("event_islamic_aspects");
        migrationBuilder.DropTable("event_tech_aspects");
        migrationBuilder.DropTable("event_session_template_custom_property_options");
        migrationBuilder.DropTable("event_session_template_custom_property_definitions");
        migrationBuilder.DropTable("event_session_templates");
        migrationBuilder.DropTable("event_template_custom_property_options");
        migrationBuilder.DropTable("event_template_custom_property_definitions");
        migrationBuilder.DropTable("event_templates");
        migrationBuilder.DropTable("events");
        migrationBuilder.DropTable("event_series");
        migrationBuilder.DropTable("event_registration_policies");
        migrationBuilder.DropTable("registration_scopes");
        migrationBuilder.DropTable("registration_modes");
        migrationBuilder.DropTable("schedule_item_kinds");
        migrationBuilder.DropTable("event_session_kinds");
        migrationBuilder.DropTable("event_session_statuses");
        migrationBuilder.DropTable("event_formats");
        migrationBuilder.DropTable("event_statuses");
        migrationBuilder.DropTable("event_types");
        // audience_ages, audience_genders, madhabs are generic lookup tables retained
        // after Event product surface removal per governance decision.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        CreateCustomPropertyProjectionTrackingTables(migrationBuilder);
        CreateLookupTables(migrationBuilder);
        CreateRootTables(migrationBuilder);
        CreateDependentTables(migrationBuilder);
        CreateIndexesAndForeignKeys(migrationBuilder);
        RestoreRetainedColumnsIndexesAndForeignKeys(migrationBuilder);
        RecreateEventWithSessionsView(migrationBuilder);
        RestoreEventImageStoragePurpose(migrationBuilder);
    }

    private static void RemoveEventImageStoragePurpose(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE storage_objects DROP CONSTRAINT IF EXISTS ck_storage_objects_purpose;
            ALTER TABLE storage_objects ADD CONSTRAINT ck_storage_objects_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'attachment', 'document', 'system_asset'));
            ALTER TABLE storage_upload_sessions DROP CONSTRAINT IF EXISTS ck_storage_upload_sessions_purpose;
            ALTER TABLE storage_upload_sessions ADD CONSTRAINT ck_storage_upload_sessions_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'attachment', 'document', 'system_asset'));
            """);
    }

    private static void RestoreEventImageStoragePurpose(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE storage_objects DROP CONSTRAINT IF EXISTS ck_storage_objects_purpose;
            ALTER TABLE storage_objects ADD CONSTRAINT ck_storage_objects_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'event_image', 'attachment', 'document', 'system_asset'));
            ALTER TABLE storage_upload_sessions DROP CONSTRAINT IF EXISTS ck_storage_upload_sessions_purpose;
            ALTER TABLE storage_upload_sessions ADD CONSTRAINT ck_storage_upload_sessions_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'event_image', 'attachment', 'document', 'system_asset'));
            """);
    }

    private static void CreateCustomPropertyProjectionTrackingTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "custom_property_projection_dirty_scope",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                definition_id = table.Column<Guid>(type: "uuid", nullable: true),
                drained_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                projection_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                projection_version = table.Column<int>(type: "integer", nullable: false),
                reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_custom_property_projection_dirty_scope", x => x.id);
                table.ForeignKey(
                    name: "fk_custom_property_projection_dirty_scope_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "custom_property_projection_status",
            columns: table => new
            {
                projection_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                projection_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false),
                last_checkpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                last_error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                last_rebuild_completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_rebuild_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                rows_failed = table.Column<long>(type: "bigint", nullable: false),
                rows_processed = table.Column<long>(type: "bigint", nullable: false),
                state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_custom_property_projection_status", x => new { x.projection_name, x.projection_version, x.tenant_id });
                table.ForeignKey(
                    name: "fk_custom_property_projection_status_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_custom_property_projection_dirty_scope_tenant_id",
            table: "custom_property_projection_dirty_scope",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_dirty_scope_pending",
            table: "custom_property_projection_dirty_scope",
            columns: new[] { "projection_name", "projection_version", "tenant_id" },
            filter: "drained_at IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_dirty_scope_unique",
            table: "custom_property_projection_dirty_scope",
            columns: new[] { "projection_name", "projection_version", "tenant_id", "scope_type", "scope_id", "definition_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_custom_property_projection_status_tenant_id",
            table: "custom_property_projection_status",
            column: "tenant_id");
    }

    private static void CreateLookupTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "event_formats", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_event_formats", x => x.id));
        migrationBuilder.CreateTable(name: "event_registration_policies", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_event_registration_policies", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_kinds", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_event_session_kinds", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_statuses", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_event_session_statuses", x => x.id));
        migrationBuilder.CreateTable(name: "event_statuses", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_event_statuses", x => x.id));
        migrationBuilder.CreateTable(name: "registration_modes", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_registration_modes", x => x.id));
        migrationBuilder.CreateTable(name: "registration_scopes", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_registration_scopes", x => x.id));
        migrationBuilder.CreateTable(name: "schedule_item_kinds", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), master_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true) }, constraints: table => table.PrimaryKey("pk_schedule_item_kinds", x => x.id));
        migrationBuilder.CreateTable(name: "event_types", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), full_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false), master_code = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false), description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true), tenant_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_types", x => x.id));
    }

    private static void CreateRootTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "event_series", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), actor_id = table.Column<Guid>(type: "uuid", nullable: false), title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true), description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true), visibility_type_id = table.Column<int>(type: "integer", nullable: false), is_published = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_series", x => x.id));
        migrationBuilder.CreateTable(name: "event_templates", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_type_id = table.Column<int>(type: "integer", nullable: true), template_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), version = table.Column<int>(type: "integer", nullable: false), is_published = table.Column<bool>(type: "boolean", nullable: false), is_active = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_templates", x => x.id));
        migrationBuilder.CreateTable(name: "events", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_type_id = table.Column<int>(type: "integer", nullable: true), event_status_id = table.Column<int>(type: "integer", nullable: false), event_format_id = table.Column<int>(type: "integer", nullable: false), event_series_id = table.Column<Guid>(type: "uuid", nullable: true), actor_id = table.Column<Guid>(type: "uuid", nullable: false), title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true), public_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false), provenance_external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true), provenance_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true), last_session_end_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), is_deleted = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_events", x => x.id); table.UniqueConstraint("ak_events_tenant_id_id", x => new { x.tenant_id, x.id }); });
    }

    private static void CreateDependentTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "event_session_templates", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_template_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), session_template_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), version = table.Column<int>(type: "integer", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_templates", x => x.id));
        migrationBuilder.CreateTable(name: "event_days", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), local_date = table.Column<DateOnly>(type: "date", nullable: false), is_published = table.Column<bool>(type: "boolean", nullable: false), sort_order = table.Column<int>(type: "integer", nullable: false), is_deleted = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_event_days", x => x.id); table.UniqueConstraint("ak_event_days_tenant_id_event_id_id", x => new { x.tenant_id, x.event_id, x.id }); table.UniqueConstraint("ak_event_days_tenant_id_id", x => new { x.tenant_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_sessions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), event_day_id = table.Column<Guid>(type: "uuid", nullable: true), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_session_status_id = table.Column<int>(type: "integer", nullable: false), event_session_kind_id = table.Column<int>(type: "integer", nullable: true), registration_mode_id = table.Column<int>(type: "integer", nullable: true), location_id = table.Column<Guid>(type: "uuid", nullable: true), room_id = table.Column<Guid>(type: "uuid", nullable: true), title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true), start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), local_start_date = table.Column<DateOnly>(type: "date", nullable: true), local_end_date = table.Column<DateOnly>(type: "date", nullable: true), local_start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true), local_end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true), local_start_minute_of_day = table.Column<int>(type: "integer", nullable: true), local_end_minute_of_day = table.Column<int>(type: "integer", nullable: true), end_time_type = table.Column<int>(type: "integer", nullable: false), is_deleted = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_event_sessions", x => x.id); table.UniqueConstraint("ak_event_sessions_tenant_id_event_id_id", x => new { x.tenant_id, x.event_id, x.id }); table.UniqueConstraint("ak_event_sessions_tenant_id_id", x => new { x.tenant_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_session_groups", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), location_id = table.Column<Guid>(type: "uuid", nullable: true), room_id = table.Column<Guid>(type: "uuid", nullable: true), name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), is_deleted = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_event_session_groups", x => x.id); table.UniqueConstraint("ak_event_session_groups_tenant_id_event_id_id", x => new { x.tenant_id, x.event_id, x.id }); table.UniqueConstraint("ak_event_session_groups_tenant_id_id", x => new { x.tenant_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_registration_intents", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), user_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), registration_scope_id = table.Column<int>(type: "integer", nullable: false), selected_event_day_id = table.Column<Guid>(type: "uuid", nullable: true), is_deleted = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_event_registration_intents", x => x.id); table.UniqueConstraint("ak_event_registration_intents_tenant_id_event_id_id", x => new { x.tenant_id, x.event_id, x.id }); table.UniqueConstraint("ak_event_registration_intents_tenant_id_id", x => new { x.tenant_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_registrations", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_registration_intent_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_registrations", x => x.id));
        migrationBuilder.CreateTable(name: "event_islamic_aspects", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), madhab_id = table.Column<int>(type: "integer", nullable: true), primary_language_id = table.Column<int>(type: "integer", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_islamic_aspects", x => x.id));
        migrationBuilder.CreateTable(name: "event_tech_aspects", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_tech_aspects", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_islamic_aspects", columns: table => new { event_session_id = table.Column<Guid>(type: "uuid", nullable: false), start_time_type = table.Column<int>(type: "integer", nullable: false), reference_prayer = table.Column<int>(type: "integer", nullable: true), offset_minutes = table.Column<int>(type: "integer", nullable: true), end_reference_prayer = table.Column<int>(type: "integer", nullable: true), end_offset_minutes = table.Column<int>(type: "integer", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_session_islamic_aspects", x => x.event_session_id));
        migrationBuilder.CreateTable(name: "event_agenda_items", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), event_day_id = table.Column<Guid>(type: "uuid", nullable: true), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), kind_id = table.Column<int>(type: "integer", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_agenda_items", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_agenda_items", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), event_agenda_item_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_agenda_items", x => x.id));
        migrationBuilder.CreateTable(name: "event_categories", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), category_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_categories", x => x.id));
        migrationBuilder.CreateTable(name: "event_tags", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tag_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_tags", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_categories", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), category_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_categories", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_tags", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), tag_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_tags", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_languages", columns: table => new { id = table.Column<int>(type: "integer", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), language_id = table.Column<int>(type: "integer", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_languages", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_speakers", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), actor_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_speakers", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_group_sessions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_group_id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_group_sessions", x => x.id));
        migrationBuilder.CreateTable(name: "event_contact_share_consents", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_contact_share_consents", x => x.id));
        migrationBuilder.CreateTable(name: "event_contact_share_exports", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_contact_share_exports", x => x.id));
        migrationBuilder.CreateTable(name: "event_contact_share_export_items", columns: table => new { export_id = table.Column<Guid>(type: "uuid", nullable: false), consent_id = table.Column<Guid>(type: "uuid", nullable: false), email_snapshot = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false) }, constraints: table => table.PrimaryKey("pk_event_contact_share_export_items", x => new { x.export_id, x.consent_id }));
        CreateCustomPropertyTables(migrationBuilder);
        CreateReportingTables(migrationBuilder);
        migrationBuilder.CreateTable(name: "event_moderation_records", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), moderator_user_id = table.Column<Guid>(type: "uuid", nullable: true), source_report_id = table.Column<Guid>(type: "uuid", nullable: true), source_report_decision_id = table.Column<Guid>(type: "uuid", nullable: true), action_kind = table.Column<int>(type: "integer", nullable: false), created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_moderation_records", x => x.id));
        migrationBuilder.CreateTable(name: "event_role_assignments", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), user_id = table.Column<Guid>(type: "uuid", nullable: false), role_id = table.Column<int>(type: "integer", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_role_assignments", x => x.id));
    }

    private static void CreateCustomPropertyTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "event_template_custom_property_definitions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_template_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), default_option_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_template_custom_property_definitions", x => x.id));
        migrationBuilder.CreateTable(name: "event_template_custom_property_options", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_template_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), parent_option_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_template_custom_property_options", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_template_custom_property_definitions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_template_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), default_option_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_session_template_custom_property_definitions", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_template_custom_property_options", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_template_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), parent_option_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_session_template_custom_property_options", x => x.id));
        migrationBuilder.CreateTable(name: "event_custom_property_definitions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), source_template_id = table.Column<Guid>(type: "uuid", nullable: true), default_option_id = table.Column<Guid>(type: "uuid", nullable: true), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_custom_property_definitions", x => x.id));
        migrationBuilder.CreateTable(name: "event_custom_property_options", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), parent_option_id = table.Column<Guid>(type: "uuid", nullable: true), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_custom_property_options", x => x.id));
        migrationBuilder.CreateTable(name: "event_custom_property_values", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), option_id = table.Column<Guid>(type: "uuid", nullable: true), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_custom_property_values", x => x.id));
        migrationBuilder.CreateTable(name: "event_custom_property_projections", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), event_custom_property_value_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), option_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_custom_property_projections", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_custom_property_definitions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), source_template_id = table.Column<Guid>(type: "uuid", nullable: true), default_option_id = table.Column<Guid>(type: "uuid", nullable: true), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_custom_property_definitions", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_custom_property_options", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), parent_option_id = table.Column<Guid>(type: "uuid", nullable: true), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_custom_property_options", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_custom_property_values", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), option_id = table.Column<Guid>(type: "uuid", nullable: true), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_session_custom_property_values", x => x.id));
        migrationBuilder.CreateTable(name: "event_session_custom_property_projections", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), event_session_custom_property_definition_id = table.Column<Guid>(type: "uuid", nullable: false), event_session_custom_property_value_id = table.Column<Guid>(type: "uuid", nullable: false), event_session_id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), option_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_session_custom_property_projections", x => x.id));
    }

    private static void CreateReportingTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "event_reports", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), event_id = table.Column<Guid>(type: "uuid", nullable: false), reporter_user_id = table.Column<Guid>(type: "uuid", nullable: true), reporter_actor_id = table.Column<Guid>(type: "uuid", nullable: true), status = table.Column<int>(type: "integer", nullable: false), priority = table.Column<int>(type: "integer", nullable: false), created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false), is_deleted = table.Column<bool>(type: "boolean", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_event_reports", x => x.id); table.UniqueConstraint("ak_event_reports_tenant_id_event_id_id", x => new { x.tenant_id, x.event_id, x.id }); table.UniqueConstraint("ak_event_reports_tenant_id_id", x => new { x.tenant_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_report_cases", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), report_id = table.Column<Guid>(type: "uuid", nullable: false), assigned_moderator_user_id = table.Column<Guid>(type: "uuid", nullable: true), status = table.Column<int>(type: "integer", nullable: false), priority = table.Column<int>(type: "integer", nullable: false), created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false), concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false) }, constraints: table => { table.PrimaryKey("pk_event_report_cases", x => x.id); table.UniqueConstraint("ak_event_report_cases_tenant_id_id", x => new { x.tenant_id, x.id }); table.UniqueConstraint("ak_event_report_cases_tenant_id_report_id_id", x => new { x.tenant_id, x.report_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_report_evidence", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), report_id = table.Column<Guid>(type: "uuid", nullable: false), storage_object_id = table.Column<Guid>(type: "uuid", nullable: true), created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_report_evidence", x => x.id));
        migrationBuilder.CreateTable(name: "event_report_targets", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), report_id = table.Column<Guid>(type: "uuid", nullable: false), storage_object_id = table.Column<Guid>(type: "uuid", nullable: true) }, constraints: table => table.PrimaryKey("pk_event_report_targets", x => x.id));
        migrationBuilder.CreateTable(name: "event_report_signals", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), report_id = table.Column<Guid>(type: "uuid", nullable: true), event_id = table.Column<Guid>(type: "uuid", nullable: false), provider = table.Column<int>(type: "integer", nullable: false), provider_target_scope = table.Column<int>(type: "integer", nullable: false), provider_target_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false) }, constraints: table => table.PrimaryKey("pk_event_report_signals", x => x.id));
        migrationBuilder.CreateTable(name: "event_report_decisions", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), case_id = table.Column<Guid>(type: "uuid", nullable: false), report_id = table.Column<Guid>(type: "uuid", nullable: false), moderator_user_id = table.Column<Guid>(type: "uuid", nullable: true), decision_source = table.Column<int>(type: "integer", nullable: false), provider_target_scope = table.Column<int>(type: "integer", nullable: false), provider_target_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), external_decision_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true) }, constraints: table => { table.PrimaryKey("pk_event_report_decisions", x => x.id); table.UniqueConstraint("ak_event_report_decisions_tenant_id_report_id_id", x => new { x.tenant_id, x.report_id, x.id }); });
        migrationBuilder.CreateTable(name: "event_report_external_links", columns: table => new { id = table.Column<Guid>(type: "uuid", nullable: false), tenant_id = table.Column<Guid>(type: "uuid", nullable: false), report_id = table.Column<Guid>(type: "uuid", nullable: false), case_id = table.Column<Guid>(type: "uuid", nullable: true), provider = table.Column<int>(type: "integer", nullable: false), provider_target_scope = table.Column<int>(type: "integer", nullable: false), provider_target_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false), correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false) }, constraints: table => table.PrimaryKey("pk_event_report_external_links", x => x.id));
    }

    private static void CreateIndexesAndForeignKeys(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex("ix_events_tenant_public_code", "events", new[] { "tenant_id", "public_code" }, unique: true);
        migrationBuilder.CreateIndex("ix_event_sessions_event_session_status_id", "event_sessions", "event_session_status_id");
        migrationBuilder.CreateIndex("ix_event_registration_intents_unique_event_scope", "event_registration_intents", new[] { "tenant_id", "event_id", "user_id" }, unique: true, filter: "registration_scope_id = 1 AND is_deleted = false");
        migrationBuilder.CreateIndex("ix_event_moderation_records_tenant_source_report", "event_moderation_records", new[] { "tenant_id", "source_report_id" }, filter: "source_report_id IS NOT NULL");
        migrationBuilder.CreateIndex("ix_event_moderation_records_tenant_source_report_decision", "event_moderation_records", new[] { "tenant_id", "source_report_decision_id" }, filter: "source_report_decision_id IS NOT NULL");

        migrationBuilder.AddForeignKey("fk_event_sessions_events_tenant_id_event_id", "event_sessions", new[] { "tenant_id", "event_id" }, "events", principalColumns: new[] { "tenant_id", "id" }, onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("fk_event_sessions_event_session_statuses_event_session_status_", "event_sessions", "event_session_status_id", "event_session_statuses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_registration_intents_events_tenant_id_event_id", "event_registration_intents", new[] { "tenant_id", "event_id" }, "events", principalColumns: new[] { "tenant_id", "id" }, onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("fk_event_registrations_event_registration_intents_event_registr", "event_registrations", "event_registration_intent_id", "event_registration_intents", principalColumn: "id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("fk_event_reports_events_tenant_id_event_id", "event_reports", new[] { "tenant_id", "event_id" }, "events", principalColumns: new[] { "tenant_id", "id" }, onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_report_cases_event_reports_tenant_id_report_id", "event_report_cases", new[] { "tenant_id", "report_id" }, "event_reports", principalColumns: new[] { "tenant_id", "id" });
        migrationBuilder.AddForeignKey("fk_event_report_decisions_event_report_cases_tenant_id_report_", "event_report_decisions", new[] { "tenant_id", "report_id", "case_id" }, "event_report_cases", principalColumns: new[] { "tenant_id", "report_id", "id" });
        migrationBuilder.AddForeignKey("fk_event_report_decisions_event_reports_tenant_id_report_id", "event_report_decisions", new[] { "tenant_id", "report_id" }, "event_reports", principalColumns: new[] { "tenant_id", "id" });
        migrationBuilder.AddForeignKey("fk_event_moderation_records_event_reports_tenant_id_source_rep", "event_moderation_records", new[] { "tenant_id", "source_report_id" }, "event_reports", principalColumns: new[] { "tenant_id", "id" }, onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_moderation_records_event_report_decisions_tenant_id_s", "event_moderation_records", new[] { "tenant_id", "source_report_id", "source_report_decision_id" }, "event_report_decisions", principalColumns: new[] { "tenant_id", "report_id", "id" }, onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_custom_property_definitions_event_custom_property_opt", "event_custom_property_definitions", "default_option_id", "event_custom_property_options", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_session_custom_property_definitions_event_session_cus", "event_session_custom_property_definitions", "default_option_id", "event_session_custom_property_options", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_session_template_custom_property_definitions_event_se", "event_session_template_custom_property_definitions", "default_option_id", "event_session_template_custom_property_options", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_event_template_custom_property_definitions_event_template_c", "event_template_custom_property_definitions", "default_option_id", "event_template_custom_property_options", principalColumn: "id", onDelete: ReferentialAction.Restrict);
    }

    private static void RestoreRetainedColumnsIndexesAndForeignKeys(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>("event_id", "email_dispatch_outbox", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("registration_intent_id", "email_dispatch_outbox", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("event_id", "integration_sync_outbox", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("registration_intent_id", "integration_sync_outbox", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("event_id", "notification_intents", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("report_id", "notification_intents", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("report_decision_id", "notification_intents", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("report_id", "notification_external_delegations", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("report_decision_id", "notification_external_delegations", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>("event_id", "organization_reviews", type: "uuid", nullable: true);

        migrationBuilder.AddColumn<bool>("events_allow_user_submitted_events_local_value", "instance_policy_sets", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_allow_user_submitted_events_override_mode", "instance_policy_sets", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("events_allow_organization_submitted_events_local_value", "instance_policy_sets", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_allow_organization_submitted_events_override_mode", "instance_policy_sets", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("events_allow_group_submitted_events_local_value", "instance_policy_sets", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_allow_group_submitted_events_override_mode", "instance_policy_sets", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("events_event_card_click_opens_detail_page_local_value", "instance_policy_sets", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_event_card_click_opens_detail_page_override_mode", "instance_policy_sets", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("modules_enable_islamic_module_local_value", "instance_policy_sets", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("modules_enable_islamic_module_override_mode", "instance_policy_sets", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("modules_enable_tech_module_local_value", "instance_policy_sets", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("modules_enable_tech_module_override_mode", "instance_policy_sets", type: "integer", nullable: false, defaultValue: 0);

        RestorePolicyColumns(migrationBuilder, "tenant_policy_sets");
        RestorePolicyColumns(migrationBuilder, "organization_policy_sets");

        migrationBuilder.CreateIndex("ix_email_dispatch_outbox_event_id", "email_dispatch_outbox", "event_id");
        migrationBuilder.CreateIndex("ix_email_dispatch_outbox_registration_intent_id", "email_dispatch_outbox", "registration_intent_id");
        migrationBuilder.CreateIndex("ix_integration_sync_outbox_event_id", "integration_sync_outbox", "event_id");
        migrationBuilder.CreateIndex("ix_integration_sync_outbox_registration_intent_id", "integration_sync_outbox", "registration_intent_id");
        migrationBuilder.CreateIndex("ix_notification_intents_event_id", "notification_intents", "event_id");
        migrationBuilder.CreateIndex("ix_notification_intents_report_id", "notification_intents", "report_id");
        migrationBuilder.CreateIndex("ix_notification_intents_report_decision_id", "notification_intents", "report_decision_id");
        migrationBuilder.CreateIndex("ix_notification_external_delegations_report_id", "notification_external_delegations", "report_id");
        migrationBuilder.CreateIndex("ix_notification_external_delegations_report_decision_id", "notification_external_delegations", "report_decision_id");
        migrationBuilder.CreateIndex("ix_organization_reviews_event_id", "organization_reviews", "event_id");

        migrationBuilder.AddForeignKey("fk_email_dispatch_outbox_events_event_id", "email_dispatch_outbox", "event_id", "events", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_email_dispatch_outbox_event_registration_intents_registrati", "email_dispatch_outbox", "registration_intent_id", "event_registration_intents", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_integration_sync_outbox_events_event_id", "integration_sync_outbox", "event_id", "events", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_integration_sync_outbox_event_registration_intents_registra", "integration_sync_outbox", "registration_intent_id", "event_registration_intents", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_notification_intents_events_event_id", "notification_intents", "event_id", "events", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_notification_intents_event_reports_report_id", "notification_intents", "report_id", "event_reports", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_notification_intents_event_report_decisions_report_decision", "notification_intents", "report_decision_id", "event_report_decisions", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_notification_external_delegations_event_reports_report_id", "notification_external_delegations", "report_id", "event_reports", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_notification_external_delegations_event_report_decisions_re", "notification_external_delegations", "report_decision_id", "event_report_decisions", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("fk_organization_reviews_events_event_id", "organization_reviews", "event_id", "events", principalColumn: "id", onDelete: ReferentialAction.Cascade);
    }

    private static void RestorePolicyColumns(MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.AddColumn<bool>("events_allow_user_submitted_events_local_value", table, type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_allow_user_submitted_events_override_mode", table, type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("events_allow_organization_submitted_events_local_value", table, type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_allow_organization_submitted_events_override_mode", table, type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("events_allow_group_submitted_events_local_value", table, type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_allow_group_submitted_events_override_mode", table, type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>("events_event_card_click_opens_detail_page_local_value", table, type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("events_event_card_click_opens_detail_page_override_mode", table, type: "integer", nullable: false, defaultValue: 0);
    }

    private static void RecreateEventWithSessionsView(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE VIEW vw_event_with_sessions AS
            SELECT
                ''::text AS aggregated_session_islamic_themes,
                CURRENT_TIMESTAMP AS created_at,
                ''::text AS description,
                ''::text AS difficulty_level,
                NULL::timestamp with time zone AS end_at,
                ''::text AS event_custom_property_facets,
                e.id AS event_id,
                ''::text AS event_session_custom_property_facets,
                NULL::timestamp with time zone AS first_session_start_at,
                false AS has_in_person_sessions,
                false AS has_virtual_sessions,
                e.is_deleted,
                NULL::boolean AS is_ramadan,
                ''::text AS islamic_theme,
                NULL::timestamp with time zone AS last_session_end_at,
                ''::text AS madhab,
                NULL::boolean AS prayer_aware,
                0 AS session_count,
                COALESCE(e.slug, '') AS slug,
                CURRENT_TIMESTAMP AS start_at,
                ''::text AS status,
                ''::text AS target_audience,
                ''::text AS tech_stack,
                e.tenant_id,
                e.title,
                NULL::timestamp with time zone AS updated_at,
                ''::text AS visibility
            FROM events AS e;
            """);
    }
}
