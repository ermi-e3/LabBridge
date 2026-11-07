using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LabBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateSequence(
                name: "inbound_message_receipt_seq");

            migrationBuilder.CreateTable(
                name: "adapter_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    settings_schema_json = table.Column<string>(type: "jsonb", nullable: false),
                    secret_slots_json = table.Column<string>(type: "jsonb", nullable: false),
                    binding_attributes_json = table.Column<string>(type: "jsonb", nullable: true),
                    supports_connection_test = table.Column<bool>(type: "boolean", nullable: false),
                    is_simulator = table.Column<bool>(type: "boolean", nullable: false),
                    adapter_version = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    supported_directions = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adapter_types", x => x.id);
                    table.CheckConstraint("ck_adapter_types_directions", "cardinality(supported_directions) >= 1");
                });

            migrationBuilder.CreateTable(
                name: "alert_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    scope = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    default_severity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    parameter_schema_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_action_definitions",
                columns: table => new
                {
                    action_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    domain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    action_class = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    failure_mode = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    detail_schema_json = table.Column<string>(type: "json", nullable: true),
                    is_traced = table.Column<bool>(type: "boolean", nullable: false),
                    trace_stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_action_definitions", x => x.action_code);
                    table.CheckConstraint("ck_audit_action_definitions_trace", "is_traced = (trace_stage IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "audit_spool_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Pending"),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_spool_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "interface_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_simulation = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interface_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "limit_definitions",
                columns: table => new
                {
                    limit_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    metric_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_message_volume = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_limit_definitions", x => x.limit_key);
                    table.UniqueConstraint("AK_limit_definitions_limit_key_kind_is_message_volume", x => new { x.limit_key, x.kind, x.is_message_volume });
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    suspension_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status_changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    status_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    default_time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "UTC"),
                    initial_admin_provisioned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.CheckConstraint("ck_organizations_code_format", "code ~ '^[a-z0-9]+(-[a-z0-9]+)*$' AND char_length(code) BETWEEN 3 AND 40");
                    table.CheckConstraint("ck_organizations_initial_admin", "status NOT IN ('Active','Suspended') OR initial_admin_provisioned");
                    table.CheckConstraint("ck_organizations_status_reason", "status NOT IN ('Suspended','Deactivated') OR status_reason IS NOT NULL");
                    table.CheckConstraint("ck_organizations_status_version", "status_version >= 1");
                    table.CheckConstraint("ck_organizations_suspension_mode", "(status = 'Suspended' AND suspension_mode IS NOT NULL) OR (status <> 'Suspended' AND suspension_mode IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "parser_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    version = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    input_format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    limits_json = table.Column<string>(type: "jsonb", nullable: true),
                    canonical_schema_version = table.Column<int>(type: "integer", nullable: false),
                    is_simulator = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    supported_message_kinds = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parser_types", x => x.id);
                    table.CheckConstraint("ck_parser_types_kinds", "cardinality(supported_message_kinds) >= 1");
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    owner_module = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Draft"),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.id);
                    table.CheckConstraint("ck_plans_published_at", "(status = 'Draft') = (published_at IS NULL)");
                    table.CheckConstraint("ck_plans_version", "version >= 1");
                });

            migrationBuilder.CreateTable(
                name: "report_definitions",
                columns: table => new
                {
                    report_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parameter_schema_json = table.Column<string>(type: "jsonb", nullable: false),
                    sources_json = table.Column<string>(type: "jsonb", nullable: false),
                    dimensions_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_definitions", x => x.report_code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    is_additive = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "setting_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    constraints_json = table.Column<string>(type: "jsonb", nullable: true),
                    default_value_json = table.Column<string>(type: "jsonb", nullable: true),
                    platform_min_json = table.Column<string>(type: "jsonb", nullable: true),
                    platform_max_json = table.Column<string>(type: "jsonb", nullable: true),
                    editable_by = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_module = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_setting_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "interface_profile_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    definition_schema_version = table.Column<int>(type: "integer", nullable: false),
                    definition_json = table.Column<string>(type: "jsonb", nullable: false),
                    release_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interface_profile_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_interface_profile_versions_interface_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "interface_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alert_rule_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    severity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    thresholds_json = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_rule_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_alert_rule_settings_alert_rules_alert_rule_id",
                        column: x => x.alert_rule_id,
                        principalTable: "alert_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alert_rule_settings_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alert_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    severity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Open"),
                    occurrence_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    affected_objects_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    acknowledged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolve_reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    resolve_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.id);
                    table.CheckConstraint("ck_alerts_acknowledged", "status <> 'Acknowledged' OR acknowledged_at IS NOT NULL");
                    table.CheckConstraint("ck_alerts_counts", "occurrence_count >= 1 AND last_seen_at >= opened_at");
                    table.CheckConstraint("ck_alerts_resolve_reason", "status <> 'Resolved' OR resolve_reason IS NOT NULL");
                    table.CheckConstraint("ck_alerts_resolved_at", "(status = 'Resolved') = (resolved_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_alerts_alert_rules_alert_rule_id",
                        column: x => x.alert_rule_id,
                        principalTable: "alert_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alerts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_integrity_checks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stream = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    range_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    range_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    first_break_json = table.Column<string>(type: "json", nullable: true),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_integrity_checks", x => x.id);
                    table.CheckConstraint("ck_audit_integrity_checks_completed", "(status = 'Running') = (completed_at IS NULL)");
                    table.CheckConstraint("ck_audit_integrity_checks_range", "range_to > range_from");
                    table.CheckConstraint("ck_audit_integrity_checks_stream", "(stream = 'Platform') = (organization_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_audit_integrity_checks_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_seals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    seal_sequence = table.Column<long>(type: "bigint", nullable: false),
                    from_event_sequence = table.Column<long>(type: "bigint", nullable: false),
                    to_event_sequence = table.Column<long>(type: "bigint", nullable: false),
                    event_count = table.Column<int>(type: "integer", nullable: false),
                    block_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    previous_block_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: true),
                    sealed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_seals", x => x.id);
                    table.CheckConstraint("ck_audit_seals_block_hash", "block_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_audit_seals_chain", "(previous_block_hash IS NULL) = (seal_sequence = 1)");
                    table.CheckConstraint("ck_audit_seals_range", "to_event_sequence >= from_event_sequence AND event_count >= 1");
                    table.CheckConstraint("ck_audit_seals_sequence", "seal_sequence >= 1");
                    table.ForeignKey(
                        name: "FK_audit_seals_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "canonical_concepts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Active"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_canonical_concepts", x => x.id);
                    table.UniqueConstraint("AK_canonical_concepts_organization_id_id", x => new { x.organization_id, x.id });
                    table.ForeignKey(
                        name: "FK_canonical_concepts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "daily_facts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fact_date = table.Column<DateOnly>(type: "date", nullable: false),
                    fact_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dimensions_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    dimensions_json = table.Column<string>(type: "jsonb", nullable: false),
                    fact_count = table.Column<long>(type: "bigint", nullable: false),
                    sum_value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    min_value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    max_value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    histogram_json = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_facts", x => x.id);
                    table.CheckConstraint("ck_daily_facts_aggregates", "(sum_value IS NULL) = (min_value IS NULL) AND (min_value IS NULL) = (max_value IS NULL)");
                    table.CheckConstraint("ck_daily_facts_count", "fact_count >= 0");
                    table.CheckConstraint("ck_daily_facts_range", "min_value IS NULL OR min_value <= max_value");
                    table.ForeignKey(
                        name: "FK_daily_facts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_systems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    system_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Active"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_systems", x => x.id);
                    table.UniqueConstraint("AK_external_systems_organization_id_id", x => new { x.organization_id, x.id });
                    table.ForeignKey(
                        name: "FK_external_systems_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    key = table.Column<string>(type: "citext", maxLength: 128, nullable: false),
                    fingerprint = table.Column<string>(type: "citext", maxLength: 128, nullable: false),
                    response_status = table.Column<int>(type: "integer", nullable: false),
                    response_body = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_records", x => x.id);
                    table.CheckConstraint("ck_idempotency_records_expiry", "expires_at > created_at");
                    table.CheckConstraint("ck_idempotency_records_key_length", "char_length(key) BETWEEN 16 AND 128");
                    table.CheckConstraint("ck_idempotency_records_status", "response_status BETWEEN 100 AND 599");
                    table.ForeignKey(
                        name: "FK_idempotency_records_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "metric_samples",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dimensions_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_samples", x => x.id);
                    table.CheckConstraint("ck_metric_samples_counter", "kind <> 'Counter' OR value >= 0");
                    table.ForeignKey(
                        name: "FK_metric_samples_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    suspension_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_version = table.Column<int>(type: "integer", nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_status_history", x => x.id);
                    table.CheckConstraint("ck_organization_status_history_change", "from_status IS NULL OR from_status <> to_status");
                    table.CheckConstraint("ck_organization_status_history_mode", "(to_status = 'Suspended') = (suspension_mode IS NOT NULL)");
                    table.CheckConstraint("ck_organization_status_history_version", "status_version >= 1");
                    table.ForeignKey(
                        name: "FK_organization_status_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "raw_payloads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: true),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    encoding_hint = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    protection_scheme = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    key_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    stored_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    purged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    purge_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_payloads", x => x.id);
                    table.UniqueConstraint("AK_raw_payloads_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_raw_payloads_content_or_purged", "(content IS NOT NULL) <> (purged_at IS NOT NULL)");
                    table.CheckConstraint("ck_raw_payloads_hash_format", "content_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_raw_payloads_size", "size_bytes > 0 AND (content IS NULL OR octet_length(content) = size_bytes)");
                    table.ForeignKey(
                        name: "FK_raw_payloads_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usage_counters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    metric_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_counters", x => x.id);
                    table.ForeignKey(
                        name: "FK_usage_counters_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    password_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    user_name = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_user_name = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_limits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_message_volume = table.Column<bool>(type: "boolean", nullable: false),
                    limit_value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    feature_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enforcement_mode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_limits", x => x.id);
                    table.CheckConstraint("ck_plan_limits_feature_flag", "kind = 'Feature' OR NOT feature_enabled");
                    table.CheckConstraint("ck_plan_limits_message_volume_block", "NOT (is_message_volume AND enforcement_mode = 'Block')");
                    table.CheckConstraint("ck_plan_limits_quantity_value", "(kind = 'Quantity') = (limit_value IS NOT NULL)");
                    table.CheckConstraint("ck_plan_limits_value_range", "limit_value IS NULL OR limit_value >= 0");
                    table.ForeignKey(
                        name: "FK_plan_limits_limit_definitions_limit_key_kind_is_message_vol~",
                        columns: x => new { x.limit_key, x.kind, x.is_message_volume },
                        principalTable: "limit_definitions",
                        principalColumns: new[] { "limit_key", "kind", "is_message_volume" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_limits_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    period_anchor_date = table.Column<DateOnly>(type: "date", nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.UniqueConstraint("AK_subscriptions_organization_id_id", x => new { x.organization_id, x.id });
                    table.ForeignKey(
                        name: "FK_subscriptions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parameters_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    includes_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    partial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    result_json = table.Column<string>(type: "jsonb", nullable: true),
                    request_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_runs", x => x.id);
                    table.CheckConstraint("ck_report_runs_completed", "(status = 'Running') = (completed_at IS NULL)");
                    table.CheckConstraint("ck_report_runs_expiry", "status <> 'Succeeded' OR expires_at IS NOT NULL");
                    table.CheckConstraint("ck_report_runs_partial", "NOT partial OR status IN ('Succeeded','Expired')");
                    table.CheckConstraint("ck_report_runs_result", "(status = 'Succeeded') = (result_json IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_report_runs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_runs_report_definitions_report_code",
                        column: x => x.report_code,
                        principalTable: "report_definitions",
                        principalColumn: "report_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    setting_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value_json = table.Column<string>(type: "jsonb", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_settings", x => x.id);
                    table.CheckConstraint("ck_organization_settings_version", "version >= 1");
                    table.ForeignKey(
                        name: "FK_organization_settings_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_settings_setting_definitions_setting_definitio~",
                        column: x => x.setting_definition_id,
                        principalTable: "setting_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "profile_parser_bindings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parser_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    config_json = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Active"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_parser_bindings", x => x.id);
                    table.ForeignKey(
                        name: "FK_profile_parser_bindings_interface_profile_versions_profile_~",
                        column: x => x.profile_version_id,
                        principalTable: "interface_profile_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_profile_parser_bindings_parser_types_parser_type_id",
                        column: x => x.parser_type_id,
                        principalTable: "parser_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alert_state_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    to_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_state_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_alert_state_history_alerts_alert_id",
                        column: x => x.alert_id,
                        principalTable: "alerts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    scope = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    stream_sequence = table.Column<long>(type: "bigint", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    actor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    target_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    target_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    outcome = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    related_ids_json = table.Column<string>(type: "json", nullable: true),
                    source_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    detail_json = table.Column<string>(type: "json", nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    time_skew_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    seal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_ids_indexed = table.Column<string>(type: "jsonb", nullable: true, computedColumnSql: "related_ids_json::jsonb", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.id);
                    table.CheckConstraint("ck_audit_events_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_audit_events_schema_version", "schema_version >= 1");
                    table.CheckConstraint("ck_audit_events_scope", "(scope = 'Platform') = (organization_id IS NULL)");
                    table.CheckConstraint("ck_audit_events_sealing", "(stream_sequence IS NULL) = (seal_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_audit_events_audit_action_definitions_action_code",
                        column: x => x.action_code,
                        principalTable: "audit_action_definitions",
                        principalColumn: "action_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_events_audit_seals_seal_id",
                        column: x => x.seal_id,
                        principalTable: "audit_seals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_events_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unit_conversion_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_concept_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_unit_concept_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_unit_concept_id = table.Column<Guid>(type: "uuid", nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: false),
                    offset_value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: false, defaultValue: 0m),
                    rounding_decimal_places = table.Column<int>(type: "integer", nullable: true),
                    rounding_mode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    evidence_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Draft"),
                    supersedes_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retired_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_conversion_rules", x => x.id);
                    table.UniqueConstraint("AK_unit_conversion_rules_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_unit_conversion_rules_approval", "(status IN ('Approved','Retired')) = (approved_at IS NOT NULL AND approved_by IS NOT NULL)");
                    table.CheckConstraint("ck_unit_conversion_rules_multiplier", "multiplier <> 0");
                    table.CheckConstraint("ck_unit_conversion_rules_places", "rounding_decimal_places IS NULL OR rounding_decimal_places BETWEEN 0 AND 18");
                    table.CheckConstraint("ck_unit_conversion_rules_retirement", "(status = 'Retired') = (retired_at IS NOT NULL AND retired_by IS NOT NULL)");
                    table.CheckConstraint("ck_unit_conversion_rules_rounding", "(rounding_decimal_places IS NULL) = (rounding_mode IS NULL)");
                    table.CheckConstraint("ck_unit_conversion_rules_supersedes", "supersedes_rule_id IS NULL OR supersedes_rule_id <> id");
                    table.CheckConstraint("ck_unit_conversion_rules_units_differ", "from_unit_concept_id <> to_unit_concept_id");
                    table.ForeignKey(
                        name: "FK_unit_conversion_rules_canonical_concepts_organization_id_fr~",
                        columns: x => new { x.organization_id, x.from_unit_concept_id },
                        principalTable: "canonical_concepts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_conversion_rules_canonical_concepts_organization_id_te~",
                        columns: x => new { x.organization_id, x.test_concept_id },
                        principalTable: "canonical_concepts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_conversion_rules_canonical_concepts_organization_id_to~",
                        columns: x => new { x.organization_id, x.to_unit_concept_id },
                        principalTable: "canonical_concepts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_conversion_rules_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_conversion_rules_unit_conversion_rules_organization_id~",
                        columns: x => new { x.organization_id, x.supersedes_rule_id },
                        principalTable: "unit_conversion_rules",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "instruments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    compatibility_class = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    compatibility_rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    compatibility_basis_profile_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verification_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unverified"),
                    order_source_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    result_destination_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    topology = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Active"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instruments", x => x.id);
                    table.UniqueConstraint("AK_instruments_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_instruments_class_basis", "(COALESCE(compatibility_class, '') = 'C1') = (compatibility_basis_profile_version_id IS NOT NULL)");
                    table.CheckConstraint("ck_instruments_class_rationale", "compatibility_class IS NULL OR compatibility_class = 'C1' OR compatibility_rationale IS NOT NULL");
                    table.CheckConstraint("ck_instruments_simulated_no_class", "NOT is_simulated OR compatibility_class IS NULL");
                    table.CheckConstraint("ck_instruments_simulated_not_hardware_verified", "NOT is_simulated OR verification_status <> 'HardwareVerified'");
                    table.CheckConstraint("ck_instruments_topology", "(topology IS NULL) = (order_source_system_id IS NULL OR result_destination_system_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_instruments_external_systems_organization_id_order_source_s~",
                        columns: x => new { x.organization_id, x.order_source_system_id },
                        principalTable: "external_systems",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_instruments_external_systems_organization_id_result_destina~",
                        columns: x => new { x.organization_id, x.result_destination_system_id },
                        principalTable: "external_systems",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_instruments_interface_profile_versions_compatibility_basis_~",
                        column: x => x.compatibility_basis_profile_version_id,
                        principalTable: "interface_profile_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_instruments_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_identities",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_identities", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_external_identities_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_invitations_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_invitations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_role_assignments",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role_assignments", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_role_assignments_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_role_assignments_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_role_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    to_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_history", x => x.id);
                    table.CheckConstraint("ck_subscription_history_from", "(from_plan_id IS NULL) = (from_status IS NULL)");
                    table.ForeignKey(
                        name: "FK_subscription_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_history_plans_from_plan_id",
                        column: x => x.from_plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_history_plans_to_plan_id",
                        column: x => x.to_plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_history_subscriptions_organization_id_subscrip~",
                        columns: x => new { x.organization_id, x.subscription_id },
                        principalTable: "subscriptions",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "connectors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    direction = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    adapter_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    settings_json = table.Column<string>(type: "jsonb", nullable: false),
                    config_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Draft"),
                    binding_key_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    ever_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connectors", x => x.id);
                    table.UniqueConstraint("AK_connectors_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_connectors_binding_required", "direction = 'Outbound' OR binding_key_hash IS NOT NULL");
                    table.CheckConstraint("ck_connectors_single_counterparty", "(instrument_id IS NULL) <> (external_system_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_connectors_adapter_types_adapter_type_id",
                        column: x => x.adapter_type_id,
                        principalTable: "adapter_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connectors_external_systems_organization_id_external_system~",
                        columns: x => new { x.organization_id, x.external_system_id },
                        principalTable: "external_systems",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connectors_instruments_organization_id_instrument_id",
                        columns: x => new { x.organization_id, x.instrument_id },
                        principalTable: "instruments",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connectors_interface_profile_versions_profile_version_id",
                        column: x => x.profile_version_id,
                        principalTable: "interface_profile_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connectors_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "instrument_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    outcome = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    evidence_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    performed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instrument_verifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_instrument_verifications_instruments_organization_id_instru~",
                        columns: x => new { x.organization_id, x.instrument_id },
                        principalTable: "instruments",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_instrument_verifications_interface_profile_versions_profile~",
                        column: x => x.profile_version_id,
                        principalTable: "interface_profile_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_instrument_verifications_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "connector_config_revisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    config_version = table.Column<int>(type: "integer", nullable: false),
                    snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connector_config_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_connector_config_revisions_connectors_organization_id_conne~",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connector_config_revisions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "connector_health_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    previous_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    last_error_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    config_version = table.Column<int>(type: "integer", nullable: false),
                    applied_config_version = table.Column<int>(type: "integer", nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    event_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connector_health_snapshots", x => x.id);
                    table.CheckConstraint("ck_connector_health_snapshots_version", "config_version >= 1");
                    table.ForeignKey(
                        name: "FK_connector_health_snapshots_connectors_organization_id_conne~",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connector_health_snapshots_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "connector_health_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Unknown"),
                    last_checked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_success_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_failure_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    consecutive_failures = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_error_summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    applied_config_version = table.Column<int>(type: "integer", nullable: true),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connector_health_states", x => x.id);
                    table.ForeignKey(
                        name: "FK_connector_health_states_connectors_organization_id_id",
                        columns: x => new { x.organization_id, x.id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connector_health_states_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "connector_secret_refs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    secret_reference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    set_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    set_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connector_secret_refs", x => x.id);
                    table.ForeignKey(
                        name: "FK_connector_secret_refs_connectors_organization_id_connector_~",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connector_secret_refs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "connector_test_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    config_version = table.Column<int>(type: "integer", nullable: false),
                    outcome = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tested_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connector_test_results", x => x.id);
                    table.CheckConstraint("ck_connector_test_results_times", "completed_at >= started_at");
                    table.ForeignKey(
                        name: "FK_connector_test_results_connectors_organization_id_connector~",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connector_test_results_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inbound_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_config_version = table.Column<int>(type: "integer", nullable: false),
                    profile_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_sequence = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('inbound_message_receipt_seq')"),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    message_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    transport_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    remote_endpoint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    payload_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    encoding_hint = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dedup_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    dedup_key_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    original_message_id = table.Column<Guid>(type: "uuid", nullable: true),
                    receipt_outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    receipt_reply_descriptor = table.Column<string>(type: "jsonb", nullable: true),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_messages", x => x.id);
                    table.UniqueConstraint("AK_inbound_messages_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_inbound_messages_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_inbound_messages_counters", "size_bytes > 0 AND connector_config_version >= 1");
                    table.CheckConstraint("ck_inbound_messages_original_required", "receipt_outcome = 'Accepted' OR original_message_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_inbound_messages_connectors_organization_id_connector_id",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_messages_inbound_messages_organization_id_original_~",
                        columns: x => new { x.organization_id, x.original_message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_messages_interface_profile_versions_profile_version~",
                        column: x => x.profile_version_id,
                        principalTable: "interface_profile_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_messages_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_messages_raw_payloads_organization_id_payload_id",
                        columns: x => new { x.organization_id, x.payload_id },
                        principalTable: "raw_payloads",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inbound_refusals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    organization_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    transport_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    remote_endpoint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    refused_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_refusals", x => x.id);
                    table.CheckConstraint("ck_inbound_refusals_size", "size_bytes IS NULL OR size_bytes >= 0");
                    table.ForeignKey(
                        name: "FK_inbound_refusals_connectors_organization_id_connector_id",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_refusals_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    message_kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    source_connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topology = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.id);
                    table.UniqueConstraint("AK_routes_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_routes_endpoints_differ", "source_connector_id <> destination_connector_id");
                    table.CheckConstraint("ck_routes_message_kind", "message_kind IN ('Order','Result')");
                    table.ForeignKey(
                        name: "FK_routes_connectors_organization_id_destination_connector_id",
                        columns: x => new { x.organization_id, x.destination_connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_routes_connectors_organization_id_source_connector_id",
                        columns: x => new { x.organization_id, x.source_connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_routes_instruments_organization_id_instrument_id",
                        columns: x => new { x.organization_id, x.instrument_id },
                        principalTable: "instruments",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_routes_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "simulator_behaviors",
                columns: table => new
                {
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remaining_count = table.Column<int>(type: "integer", nullable: false),
                    delay_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulator_behaviors", x => x.connector_id);
                    table.CheckConstraint("ck_simulator_behaviors_delay", "delay_seconds BETWEEN 0 AND 3600");
                    table.CheckConstraint("ck_simulator_behaviors_remaining", "remaining_count >= 0");
                    table.ForeignKey(
                        name: "FK_simulator_behaviors_connectors_organization_id_connector_id",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_simulator_behaviors_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "deduplication_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dedup_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    first_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deduplication_keys", x => x.id);
                    table.ForeignKey(
                        name: "FK_deduplication_keys_connectors_organization_id_connector_id",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_deduplication_keys_inbound_messages_organization_id_first_m~",
                        columns: x => new { x.organization_id, x.first_message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_deduplication_keys_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inbound_message_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actor_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_message_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_inbound_message_status_history_inbound_messages_organizatio~",
                        columns: x => new { x.organization_id, x.message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_message_status_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inbound_processing_handoffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Pending"),
                    hold_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receipt_sequence = table.Column<long>(type: "bigint", nullable: false),
                    available_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    claimed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_processing_handoffs", x => x.id);
                    table.CheckConstraint("ck_handoffs_claim", "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)");
                    table.CheckConstraint("ck_handoffs_hold", "(status = 'Held') = (hold_reason IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_inbound_processing_handoffs_inbound_messages_organization_i~",
                        columns: x => new { x.organization_id, x.message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_processing_handoffs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "processing_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    trigger = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    outcome = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    reason_code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    message_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    profile_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    binding_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parser_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parser_version = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    canonical_schema_version = table.Column<int>(type: "integer", nullable: true),
                    worker_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    error_summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_runs", x => x.id);
                    table.UniqueConstraint("AK_processing_runs_organization_id_id", x => new { x.organization_id, x.id });
                    table.UniqueConstraint("AK_processing_runs_organization_id_message_id_id", x => new { x.organization_id, x.message_id, x.id });
                    table.CheckConstraint("ck_processing_runs_in_progress", "status <> 'InProgress' OR (worker_id IS NOT NULL AND started_at IS NOT NULL)");
                    table.CheckConstraint("ck_processing_runs_outcome", "(outcome IS NOT NULL) = (status = 'Completed')");
                    table.CheckConstraint("ck_processing_runs_scheduled", "status <> 'Scheduled' OR next_attempt_at IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_processing_runs_inbound_messages_organization_id_message_id",
                        columns: x => new { x.organization_id, x.message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processing_runs_interface_profile_versions_profile_version_~",
                        column: x => x.profile_version_id,
                        principalTable: "interface_profile_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processing_runs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processing_runs_parser_types_parser_type_id",
                        column: x => x.parser_type_id,
                        principalTable: "parser_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processing_runs_profile_parser_bindings_binding_id",
                        column: x => x.binding_id,
                        principalTable: "profile_parser_bindings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Pending"),
                    receipt_sequence = table.Column<long>(type: "bigint", nullable: false),
                    available_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    claimed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_requests", x => x.id);
                    table.UniqueConstraint("AK_delivery_requests_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_delivery_requests_counters", "attempt_count >= 0 AND payload_version >= 1 AND receipt_sequence > 0");
                    table.CheckConstraint("ck_delivery_requests_lease", "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_delivery_requests_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_requests_routes_organization_id_route_id",
                        columns: x => new { x.organization_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    resolution_count = table.Column<int>(type: "integer", nullable: false),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_runs", x => x.id);
                    table.UniqueConstraint("AK_mapping_runs_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_mapping_runs_counts", "resolution_count >= 0 AND failure_count >= 0 AND failure_count <= resolution_count");
                    table.CheckConstraint("ck_mapping_runs_status", "(status = 'Failed') = (failure_count > 0)");
                    table.CheckConstraint("ck_mapping_runs_times", "completed_at >= started_at");
                    table.ForeignKey(
                        name: "FK_mapping_runs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_runs_routes_organization_id_route_id",
                        columns: x => new { x.organization_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "route_delivery_policies",
                columns: table => new
                {
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    base_delay_seconds = table.Column<int>(type: "integer", nullable: false),
                    max_delay_seconds = table.Column<int>(type: "integer", nullable: false),
                    jitter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    send_timeout_seconds = table.Column<int>(type: "integer", nullable: false),
                    ack_timeout_seconds = table.Column<int>(type: "integer", nullable: false),
                    reconciliation_escalation_minutes = table.Column<int>(type: "integer", nullable: false),
                    max_concurrency = table.Column<int>(type: "integer", nullable: false),
                    circuit_failure_threshold = table.Column<int>(type: "integer", nullable: true),
                    circuit_pause_seconds = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_delivery_policies", x => x.route_id);
                    table.CheckConstraint("ck_route_delivery_policies_attempts", "max_attempts >= 1");
                    table.CheckConstraint("ck_route_delivery_policies_circuit", "(circuit_failure_threshold IS NULL) = (circuit_pause_seconds IS NULL) AND COALESCE(circuit_failure_threshold, 1) >= 1 AND COALESCE(circuit_pause_seconds, 1) > 0");
                    table.CheckConstraint("ck_route_delivery_policies_concurrency", "max_concurrency >= 1");
                    table.CheckConstraint("ck_route_delivery_policies_delays", "base_delay_seconds >= 0 AND max_delay_seconds >= base_delay_seconds");
                    table.CheckConstraint("ck_route_delivery_policies_escalation", "reconciliation_escalation_minutes > 0");
                    table.CheckConstraint("ck_route_delivery_policies_timeouts", "send_timeout_seconds > 0 AND ack_timeout_seconds > 0");
                    table.ForeignKey(
                        name: "FK_route_delivery_policies_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_route_delivery_policies_routes_organization_id_route_id",
                        columns: x => new { x.organization_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "normalized_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    message_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    canonical_schema_version = table.Column<int>(type: "integer", nullable: false),
                    content_json = table.Column<string>(type: "jsonb", nullable: true),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    consumer = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    purged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_normalized_messages", x => x.id);
                    table.UniqueConstraint("AK_normalized_messages_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_normalized_messages_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_normalized_messages_purged", "(content_json IS NULL) = (purged_at IS NOT NULL)");
                    table.CheckConstraint("ck_normalized_messages_version", "version >= 1");
                    table.ForeignKey(
                        name: "FK_normalized_messages_inbound_messages_organization_id_messag~",
                        columns: x => new { x.organization_id, x.message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_normalized_messages_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_normalized_messages_processing_runs_organization_id_message~",
                        columns: x => new { x.organization_id, x.message_id, x.run_id },
                        principalTable: "processing_runs",
                        principalColumns: new[] { "organization_id", "message_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "processing_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: false),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_processing_reviews_inbound_messages_organization_id_message~",
                        columns: x => new { x.organization_id, x.message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processing_reviews_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processing_reviews_processing_runs_organization_id_message_~",
                        columns: x => new { x.organization_id, x.message_id, x.run_id },
                        principalTable: "processing_runs",
                        principalColumns: new[] { "organization_id", "message_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "validation_findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    category = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    field_path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    line_number = table.Column<int>(type: "integer", nullable: true),
                    occurrence_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validation_findings", x => x.id);
                    table.CheckConstraint("ck_validation_findings_counts", "occurrence_count >= 1 AND (line_number IS NULL OR line_number >= 1)");
                    table.ForeignKey(
                        name: "FK_validation_findings_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_validation_findings_processing_runs_organization_id_message~",
                        columns: x => new { x.organization_id, x.message_id, x.run_id },
                        principalTable: "processing_runs",
                        principalColumns: new[] { "organization_id", "message_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_version = table.Column<int>(type: "integer", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    hold_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    quarantined_from_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    receipt_sequence = table.Column<long>(type: "bigint", nullable: false),
                    claimed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_items", x => x.id);
                    table.UniqueConstraint("AK_delivery_items_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_delivery_items_counters", "attempt_count >= 0 AND payload_version >= 1 AND receipt_sequence > 0");
                    table.CheckConstraint("ck_delivery_items_hold", "hold_reason IS NULL OR state IN ('Pending','RetryScheduled')");
                    table.CheckConstraint("ck_delivery_items_lease", "(state = 'InProgress') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)");
                    table.CheckConstraint("ck_delivery_items_quarantine", "(quarantined_from_state IS NOT NULL) = (state = 'Quarantined')");
                    table.CheckConstraint("ck_delivery_items_retry", "state <> 'RetryScheduled' OR next_attempt_at IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_delivery_items_connectors_organization_id_connector_id",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_items_delivery_requests_organization_id_delivery_r~",
                        columns: x => new { x.organization_id, x.delivery_request_id },
                        principalTable: "delivery_requests",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_items_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_items_routes_organization_id_route_id",
                        columns: x => new { x.organization_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_order",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_external_system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_instrument_id = table.Column<Guid>(type: "uuid", nullable: true),
                    placer_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    filler_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    accession_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specimen_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    message_control_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    patient_external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    patient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    patient_date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    patient_sex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    specimen_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specimen_type_canonical = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specimen_type_target = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specimen_collected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    specimen_collected_at_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    priority_canonical = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    priority_target = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ordered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ordered_at_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Received"),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quarantine_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    quarantined_from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    conflicts_with_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    normalized_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    mapping_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_order", x => x.id);
                    table.UniqueConstraint("AK_lab_order_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_lab_order_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_lab_order_payload_version", "payload_version >= 1");
                    table.CheckConstraint("ck_lab_order_quarantine", "status <> 'Quarantined' OR (quarantine_reason IS NOT NULL AND quarantined_from_status IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_lab_order_connectors_organization_id_source_connector_id",
                        columns: x => new { x.organization_id, x.source_connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_external_systems_organization_id_source_external_~",
                        columns: x => new { x.organization_id, x.source_external_system_id },
                        principalTable: "external_systems",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_inbound_messages_organization_id_source_message_id",
                        columns: x => new { x.organization_id, x.source_message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_instruments_organization_id_target_instrument_id",
                        columns: x => new { x.organization_id, x.target_instrument_id },
                        principalTable: "instruments",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_lab_order_organization_id_conflicts_with_order_id",
                        columns: x => new { x.organization_id, x.conflicts_with_order_id },
                        principalTable: "lab_order",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_mapping_runs_organization_id_mapping_run_id",
                        columns: x => new { x.organization_id, x.mapping_run_id },
                        principalTable: "mapping_runs",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_normalized_messages_organization_id_normalized_me~",
                        columns: x => new { x.organization_id, x.normalized_message_id },
                        principalTable: "normalized_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_routes_organization_id_route_id",
                        columns: x => new { x.organization_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "normalized_handoffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    normalized_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Pending"),
                    receipt_sequence = table.Column<long>(type: "bigint", nullable: false),
                    available_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    claimed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_normalized_handoffs", x => x.id);
                    table.CheckConstraint("ck_normalized_handoffs_lease", "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_normalized_handoffs_normalized_messages_organization_id_nor~",
                        columns: x => new { x.organization_id, x.normalized_message_id },
                        principalTable: "normalized_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_normalized_handoffs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    control_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    transport_outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ack_outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    acceptance_outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attempt_result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    error_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    connector_config_version = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_attempts", x => x.id);
                    table.UniqueConstraint("AK_delivery_attempts_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_delivery_attempts_completed", "(attempt_result = 'InProgress') = (completed_at IS NULL)");
                    table.ForeignKey(
                        name: "FK_delivery_attempts_delivery_items_organization_id_delivery_id",
                        columns: x => new { x.organization_id, x.delivery_id },
                        principalTable: "delivery_items",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_attempts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_order_line",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    external_test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_code_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    canonical_test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    instrument_test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_order_line", x => x.id);
                    table.UniqueConstraint("AK_lab_order_line_organization_id_id", x => new { x.organization_id, x.id });
                    table.ForeignKey(
                        name: "FK_lab_order_line_lab_order_organization_id_order_id",
                        columns: x => new { x.organization_id, x.order_id },
                        principalTable: "lab_order",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_line_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_order_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actor_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    delivery_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_order_status_history_lab_order_organization_id_order_id",
                        columns: x => new { x.organization_id, x.order_id },
                        principalTable: "lab_order",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_order_status_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_result",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_external_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    correlation_method = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    correlation_basis = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correlated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    correlated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    placer_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    filler_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    accession_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specimen_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    message_control_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Received"),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quarantine_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    quarantined_from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    possible_correction = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    source_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    normalized_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    mapping_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_simulated = table.Column<bool>(type: "boolean", nullable: false),
                    enriched_fields = table.Column<List<string>>(type: "text[]", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_result", x => x.id);
                    table.UniqueConstraint("AK_lab_result_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_lab_result_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_lab_result_correlation", "(correlation_status = 'Matched') = (order_id IS NOT NULL)");
                    table.CheckConstraint("ck_lab_result_correlation_evidence", "(order_id IS NOT NULL) = (correlation_method IS NOT NULL AND correlated_at IS NOT NULL)");
                    table.CheckConstraint("ck_lab_result_payload_version", "payload_version >= 1");
                    table.CheckConstraint("ck_lab_result_quarantine", "status <> 'Quarantined' OR (quarantine_reason IS NOT NULL AND quarantined_from_status IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_lab_result_connectors_organization_id_source_connector_id",
                        columns: x => new { x.organization_id, x.source_connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_external_systems_organization_id_destination_ext~",
                        columns: x => new { x.organization_id, x.destination_external_system_id },
                        principalTable: "external_systems",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_inbound_messages_organization_id_source_message_~",
                        columns: x => new { x.organization_id, x.source_message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_instruments_organization_id_source_instrument_id",
                        columns: x => new { x.organization_id, x.source_instrument_id },
                        principalTable: "instruments",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_lab_order_organization_id_order_id",
                        columns: x => new { x.organization_id, x.order_id },
                        principalTable: "lab_order",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_mapping_runs_organization_id_mapping_run_id",
                        columns: x => new { x.organization_id, x.mapping_run_id },
                        principalTable: "mapping_runs",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_normalized_messages_organization_id_normalized_m~",
                        columns: x => new { x.organization_id, x.normalized_message_id },
                        principalTable: "normalized_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_routes_organization_id_route_id",
                        columns: x => new { x.organization_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acknowledgment_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acknowledges_control_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ack_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    acceptance_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    error_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    match_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    matched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acknowledgment_records", x => x.id);
                    table.CheckConstraint("ck_acknowledgment_records_match", "(match_status = 'Matched') = (attempt_id IS NOT NULL)");
                    table.CheckConstraint("ck_acknowledgment_records_matched_at", "(match_status = 'Matched') = (matched_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_acknowledgment_records_delivery_attempts_organization_id_at~",
                        columns: x => new { x.organization_id, x.attempt_id },
                        principalTable: "delivery_attempts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acknowledgment_records_inbound_messages_organization_id_mes~",
                        columns: x => new { x.organization_id, x.message_id },
                        principalTable: "inbound_messages",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acknowledgment_records_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Open"),
                    resolution = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    resolution_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    evidence_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    escalated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation_cases", x => x.id);
                    table.CheckConstraint("ck_reconciliation_cases_resolution", "(resolution IS NOT NULL) = (status = 'Resolved')");
                    table.CheckConstraint("ck_reconciliation_cases_resolved_at", "(resolved_at IS NOT NULL) = (status = 'Resolved')");
                    table.ForeignKey(
                        name: "FK_reconciliation_cases_delivery_attempts_organization_id_atte~",
                        columns: x => new { x.organization_id, x.attempt_id },
                        principalTable: "delivery_attempts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reconciliation_cases_delivery_items_organization_id_deliver~",
                        columns: x => new { x.organization_id, x.delivery_id },
                        principalTable: "delivery_items",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reconciliation_cases_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "simulator_sink_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: true),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    purged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulator_sink_messages", x => x.id);
                    table.CheckConstraint("ck_simulator_sink_messages_purged", "(content IS NULL) = (purged_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_simulator_sink_messages_connectors_organization_id_connecto~",
                        columns: x => new { x.organization_id, x.connector_id },
                        principalTable: "connectors",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_simulator_sink_messages_delivery_attempts_organization_id_a~",
                        columns: x => new { x.organization_id, x.attempt_id },
                        principalTable: "delivery_attempts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_simulator_sink_messages_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_observation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_line_id = table.Column<Guid>(type: "uuid", nullable: true),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    external_test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_code_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    canonical_test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    destination_test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    value_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    original_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    numeric_value = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    original_unit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    canonical_unit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    destination_unit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    destination_value_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    destination_value_numeric = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    conversion_applied = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    conversion_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    instrument_reference_range_text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observation_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    observed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    observed_at_raw = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    instrument_flags = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_observation", x => x.id);
                    table.UniqueConstraint("AK_lab_observation_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_lab_observation_conversion", "conversion_applied = (conversion_rule_id IS NOT NULL)");
                    table.CheckConstraint("ck_lab_observation_line", "line_number >= 1");
                    table.ForeignKey(
                        name: "FK_lab_observation_lab_order_line_organization_id_order_line_id",
                        columns: x => new { x.organization_id, x.order_line_id },
                        principalTable: "lab_order_line",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_observation_lab_result_organization_id_result_id",
                        columns: x => new { x.organization_id, x.result_id },
                        principalTable: "lab_result",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_observation_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_observation_unit_conversion_rules_organization_id_conve~",
                        columns: x => new { x.organization_id, x.conversion_rule_id },
                        principalTable: "unit_conversion_rules",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_result_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actor_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    delivery_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_result_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_result_status_history_lab_result_organization_id_result~",
                        columns: x => new { x.organization_id, x.result_id },
                        principalTable: "lab_result",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_result_status_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    external_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_code_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    concept_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_unit_concept_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_mapping_entries_canonical_concepts_organization_id_concept_~",
                        columns: x => new { x.organization_id, x.concept_id },
                        principalTable: "canonical_concepts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_entries_canonical_concepts_organization_id_target_u~",
                        columns: x => new { x.organization_id, x.target_unit_concept_id },
                        principalTable: "canonical_concepts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_entries_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_gap_subjects",
                columns: table => new
                {
                    gap_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_gap_subjects", x => new { x.gap_id, x.subject_type, x.subject_id });
                    table.ForeignKey(
                        name: "FK_mapping_gap_subjects_mapping_runs_organization_id_run_id",
                        columns: x => new { x.organization_id, x.run_id },
                        principalTable: "mapping_runs",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_gap_subjects_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_gaps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mapping_set_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counterparty_kind = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    domain = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    gap_kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    code_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Open"),
                    occurrence_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolved_by_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ignored_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ignored_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_gaps", x => x.id);
                    table.UniqueConstraint("AK_mapping_gaps_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_mapping_gaps_counts", "occurrence_count >= 1 AND last_seen_at >= first_seen_at");
                    table.CheckConstraint("ck_mapping_gaps_ignored", "status <> 'Ignored' OR (ignored_reason IS NOT NULL AND ignored_by IS NOT NULL)");
                    table.CheckConstraint("ck_mapping_gaps_resolved", "(status = 'Resolved') = (resolved_at IS NOT NULL AND resolved_by_version_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_mapping_gaps_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_resolutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    domain = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    source_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_code_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolved_concept_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution_type = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    conversion_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conversion_applied = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    original_numeric = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true),
                    converted_numeric = table.Column<decimal>(type: "numeric(38,18)", precision: 38, scale: 18, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_resolutions", x => x.id);
                    table.CheckConstraint("ck_mapping_resolutions_conversion", "conversion_applied = (conversion_rule_id IS NOT NULL)");
                    table.CheckConstraint("ck_mapping_resolutions_failure", "(resolution_type = 'Failed') = (failure_reason IS NOT NULL)");
                    table.CheckConstraint("ck_mapping_resolutions_line", "line_number >= 0");
                    table.ForeignKey(
                        name: "FK_mapping_resolutions_canonical_concepts_organization_id_reso~",
                        columns: x => new { x.organization_id, x.resolved_concept_id },
                        principalTable: "canonical_concepts",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_resolutions_mapping_runs_organization_id_run_id",
                        columns: x => new { x.organization_id, x.run_id },
                        principalTable: "mapping_runs",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_resolutions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_resolutions_unit_conversion_rules_organization_id_c~",
                        columns: x => new { x.organization_id, x.conversion_rule_id },
                        principalTable: "unit_conversion_rules",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_set_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mapping_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    unmatched_policy = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false, defaultValue: "Fail"),
                    change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    copied_from_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    published_by = table.Column<Guid>(type: "uuid", nullable: true),
                    direction = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_set_versions", x => x.id);
                    table.UniqueConstraint("AK_mapping_set_versions_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_mapping_set_versions_draft", "(status = 'Draft') = (published_at IS NULL)");
                    table.CheckConstraint("ck_mapping_set_versions_number", "version_number >= 1");
                    table.CheckConstraint("ck_mapping_set_versions_published", "status NOT IN ('Published','Superseded') OR (change_reason IS NOT NULL AND published_at IS NOT NULL)");
                    table.CheckConstraint("ck_mapping_set_versions_published_by", "(published_at IS NULL) = (published_by IS NULL)");
                    table.ForeignKey(
                        name: "FK_mapping_set_versions_mapping_set_versions_organization_id_c~",
                        columns: x => new { x.organization_id, x.copied_from_version_id },
                        principalTable: "mapping_set_versions",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_set_versions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    counterparty_kind = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    direction = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    domain = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    active_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Active"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_sets", x => x.id);
                    table.UniqueConstraint("AK_mapping_sets_organization_id_id", x => new { x.organization_id, x.id });
                    table.CheckConstraint("ck_mapping_sets_counterparty", "(instrument_id IS NULL) <> (external_system_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_mapping_sets_external_systems_organization_id_external_syst~",
                        columns: x => new { x.organization_id, x.external_system_id },
                        principalTable: "external_systems",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_sets_instruments_organization_id_instrument_id",
                        columns: x => new { x.organization_id, x.instrument_id },
                        principalTable: "instruments",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_sets_mapping_set_versions_organization_id_active_ve~",
                        columns: x => new { x.organization_id, x.active_version_id },
                        principalTable: "mapping_set_versions",
                        principalColumns: new[] { "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mapping_sets_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acknowledgment_records_message_id",
                table: "acknowledgment_records",
                column: "message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acknowledgment_records_organization_id_acknowledges_control~",
                table: "acknowledgment_records",
                columns: new[] { "organization_id", "acknowledges_control_id" });

            migrationBuilder.CreateIndex(
                name: "IX_acknowledgment_records_organization_id_attempt_id",
                table: "acknowledgment_records",
                columns: new[] { "organization_id", "attempt_id" });

            migrationBuilder.CreateIndex(
                name: "IX_acknowledgment_records_organization_id_match_status_receive~",
                table: "acknowledgment_records",
                columns: new[] { "organization_id", "match_status", "received_at" });

            migrationBuilder.CreateIndex(
                name: "IX_acknowledgment_records_organization_id_message_id",
                table: "acknowledgment_records",
                columns: new[] { "organization_id", "message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_adapter_types_code",
                table: "adapter_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_rule_settings_alert_rule_id",
                table: "alert_rule_settings",
                column: "alert_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_alert_rule_settings_organization_id_alert_rule_id",
                table: "alert_rule_settings",
                columns: new[] { "organization_id", "alert_rule_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_rules_rule_code",
                table: "alert_rules",
                column: "rule_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_state_history_alert_id_changed_at",
                table: "alert_state_history",
                columns: new[] { "alert_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_alerts_organization_id_status_opened_at",
                table: "alerts",
                columns: new[] { "organization_id", "status", "opened_at" });

            migrationBuilder.CreateIndex(
                name: "ux_alerts_active_org",
                table: "alerts",
                columns: new[] { "organization_id", "alert_rule_id", "scope_key" },
                unique: true,
                filter: "status IN ('Open','Acknowledged') AND organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_alerts_active_platform",
                table: "alerts",
                columns: new[] { "alert_rule_id", "scope_key" },
                unique: true,
                filter: "status IN ('Open','Acknowledged') AND organization_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_action_code",
                table: "audit_events",
                column: "action_code");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_organization_id_action_code_occurred_at",
                table: "audit_events",
                columns: new[] { "organization_id", "action_code", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_organization_id_correlation_id",
                table: "audit_events",
                columns: new[] { "organization_id", "correlation_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_organization_id_occurred_at",
                table: "audit_events",
                columns: new[] { "organization_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_organization_id_target_type_target_id",
                table: "audit_events",
                columns: new[] { "organization_id", "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_related_ids_indexed",
                table: "audit_events",
                column: "related_ids_indexed")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_seal_id",
                table: "audit_events",
                column: "seal_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_unsealed",
                table: "audit_events",
                column: "recorded_at",
                filter: "seal_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_audit_events_org_sequence",
                table: "audit_events",
                columns: new[] { "organization_id", "stream_sequence" },
                unique: true,
                filter: "stream_sequence IS NOT NULL AND organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_audit_events_platform_sequence",
                table: "audit_events",
                column: "stream_sequence",
                unique: true,
                filter: "stream_sequence IS NOT NULL AND organization_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_audit_integrity_checks_organization_id_started_at",
                table: "audit_integrity_checks",
                columns: new[] { "organization_id", "started_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ux_audit_seals_org_sequence",
                table: "audit_seals",
                columns: new[] { "organization_id", "seal_sequence" },
                unique: true,
                filter: "organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_audit_seals_platform_sequence",
                table: "audit_seals",
                column: "seal_sequence",
                unique: true,
                filter: "organization_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_audit_spool_items_audit_event_id",
                table: "audit_spool_items",
                column: "audit_event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_spool_items_pending",
                table: "audit_spool_items",
                column: "next_attempt_at",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_audit_spool_items_status_next_attempt_at",
                table: "audit_spool_items",
                columns: new[] { "status", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "IX_canonical_concepts_organization_id_domain_code",
                table: "canonical_concepts",
                columns: new[] { "organization_id", "domain", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connector_config_revisions_connector_id_config_version",
                table: "connector_config_revisions",
                columns: new[] { "connector_id", "config_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connector_config_revisions_organization_id_connector_id",
                table: "connector_config_revisions",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_connector_health_snapshots_connector_id_event_key",
                table: "connector_health_snapshots",
                columns: new[] { "connector_id", "event_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connector_health_snapshots_connector_id_occurred_at",
                table: "connector_health_snapshots",
                columns: new[] { "connector_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_connector_health_snapshots_organization_id_connector_id",
                table: "connector_health_snapshots",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "ix_connector_health_snapshots_retention",
                table: "connector_health_snapshots",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_connector_health_states_organization_id_id",
                table: "connector_health_states",
                columns: new[] { "organization_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connector_secret_refs_connector_id_slot",
                table: "connector_secret_refs",
                columns: new[] { "connector_id", "slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connector_secret_refs_organization_id_connector_id",
                table: "connector_secret_refs",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_connector_test_results_connector_id_started_at",
                table: "connector_test_results",
                columns: new[] { "connector_id", "started_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_connector_test_results_current_config",
                table: "connector_test_results",
                columns: new[] { "connector_id", "config_version" });

            migrationBuilder.CreateIndex(
                name: "IX_connector_test_results_organization_id_connector_id",
                table: "connector_test_results",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_connectors_adapter_type_id",
                table: "connectors",
                column: "adapter_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_connectors_organization_id_external_system_id",
                table: "connectors",
                columns: new[] { "organization_id", "external_system_id" });

            migrationBuilder.CreateIndex(
                name: "IX_connectors_organization_id_instrument_id",
                table: "connectors",
                columns: new[] { "organization_id", "instrument_id" });

            migrationBuilder.CreateIndex(
                name: "IX_connectors_organization_id_name",
                table: "connectors",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connectors_organization_id_status",
                table: "connectors",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_connectors_profile_version_id",
                table: "connectors",
                column: "profile_version_id");

            migrationBuilder.CreateIndex(
                name: "ux_connectors_binding_key_hash",
                table: "connectors",
                column: "binding_key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_facts_fact_date",
                table: "daily_facts",
                column: "fact_date");

            migrationBuilder.CreateIndex(
                name: "ux_daily_facts_key",
                table: "daily_facts",
                columns: new[] { "organization_id", "fact_date", "fact_type", "dimensions_key", "is_simulated" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_keys_expires_at",
                table: "deduplication_keys",
                column: "expires_at",
                filter: "expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_keys_organization_id_first_message_id",
                table: "deduplication_keys",
                columns: new[] { "organization_id", "first_message_id" });

            migrationBuilder.CreateIndex(
                name: "ux_deduplication_keys_claim",
                table: "deduplication_keys",
                columns: new[] { "organization_id", "connector_id", "dedup_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_attempts_delivery_id_attempt_number",
                table: "delivery_attempts",
                columns: new[] { "delivery_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_attempts_organization_id_control_id",
                table: "delivery_attempts",
                columns: new[] { "organization_id", "control_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_attempts_organization_id_delivery_id",
                table: "delivery_attempts",
                columns: new[] { "organization_id", "delivery_id" });

            migrationBuilder.CreateIndex(
                name: "ux_delivery_attempts_one_in_progress",
                table: "delivery_attempts",
                column: "delivery_id",
                unique: true,
                filter: "attempt_result = 'InProgress'");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_delivery_request_id",
                table: "delivery_items",
                column: "delivery_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delivery_items_dispatchable",
                table: "delivery_items",
                column: "receipt_sequence",
                filter: "state IN ('Pending','RetryScheduled') AND hold_reason IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_lease_expires_at",
                table: "delivery_items",
                column: "lease_expires_at",
                filter: "state = 'InProgress'");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_organization_id_connector_id",
                table: "delivery_items",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_organization_id_delivery_request_id",
                table: "delivery_items",
                columns: new[] { "organization_id", "delivery_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_organization_id_idempotency_key",
                table: "delivery_items",
                columns: new[] { "organization_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_organization_id_route_id",
                table: "delivery_items",
                columns: new[] { "organization_id", "route_id" });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_organization_id_state_created_at",
                table: "delivery_items",
                columns: new[] { "organization_id", "state", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_delivery_items_route_in_progress",
                table: "delivery_items",
                column: "route_id",
                filter: "state = 'InProgress'");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_state_next_attempt_at_receipt_sequence",
                table: "delivery_items",
                columns: new[] { "state", "next_attempt_at", "receipt_sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_items_subject_type_subject_id",
                table: "delivery_items",
                columns: new[] { "subject_type", "subject_id" });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_requests_lease_expires_at",
                table: "delivery_requests",
                column: "lease_expires_at",
                filter: "status = 'Claimed'");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_requests_organization_id_route_id",
                table: "delivery_requests",
                columns: new[] { "organization_id", "route_id" });

            migrationBuilder.CreateIndex(
                name: "ix_delivery_requests_pending",
                table: "delivery_requests",
                column: "receipt_sequence",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_requests_status_available_at_receipt_sequence",
                table: "delivery_requests",
                columns: new[] { "status", "available_at", "receipt_sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_requests_subject_type_subject_id_payload_version",
                table: "delivery_requests",
                columns: new[] { "subject_type", "subject_id", "payload_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_identities_user_id",
                table: "external_identities",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_external_systems_organization_id_name",
                table: "external_systems",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_records_expires_at",
                table: "idempotency_records",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ux_idempotency_org_key",
                table: "idempotency_records",
                columns: new[] { "organization_id", "key" },
                unique: true,
                filter: "organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_idempotency_platform_key",
                table: "idempotency_records",
                column: "key",
                unique: true,
                filter: "organization_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_message_status_history_message_id_changed_at",
                table: "inbound_message_status_history",
                columns: new[] { "message_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_message_status_history_organization_id_message_id",
                table: "inbound_message_status_history",
                columns: new[] { "organization_id", "message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_correlation_id",
                table: "inbound_messages",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_organization_id_connector_id_received_at",
                table: "inbound_messages",
                columns: new[] { "organization_id", "connector_id", "received_at" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_organization_id_content_hash",
                table: "inbound_messages",
                columns: new[] { "organization_id", "content_hash" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_organization_id_original_message_id",
                table: "inbound_messages",
                columns: new[] { "organization_id", "original_message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_organization_id_payload_id",
                table: "inbound_messages",
                columns: new[] { "organization_id", "payload_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_organization_id_received_at",
                table: "inbound_messages",
                columns: new[] { "organization_id", "received_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_organization_id_status_received_at",
                table: "inbound_messages",
                columns: new[] { "organization_id", "status", "received_at" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_profile_version_id",
                table: "inbound_messages",
                column: "profile_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_messages_receipt_sequence",
                table: "inbound_messages",
                column: "receipt_sequence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inbound_handoffs_pending",
                table: "inbound_processing_handoffs",
                column: "receipt_sequence",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_processing_handoffs_lease_expires_at",
                table: "inbound_processing_handoffs",
                column: "lease_expires_at",
                filter: "status = 'Claimed'");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_processing_handoffs_message_id",
                table: "inbound_processing_handoffs",
                column: "message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbound_processing_handoffs_organization_id_message_id",
                table: "inbound_processing_handoffs",
                columns: new[] { "organization_id", "message_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbound_processing_handoffs_organization_id_status_receipt_~",
                table: "inbound_processing_handoffs",
                columns: new[] { "organization_id", "status", "receipt_sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_processing_handoffs_status_available_at_receipt_seq~",
                table: "inbound_processing_handoffs",
                columns: new[] { "status", "available_at", "receipt_sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_refusals_connector_id_refused_at",
                table: "inbound_refusals",
                columns: new[] { "connector_id", "refused_at" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_refusals_organization_id_connector_id",
                table: "inbound_refusals",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_refusals_organization_id_refused_at",
                table: "inbound_refusals",
                columns: new[] { "organization_id", "refused_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_instrument_verifications_organization_id_instrument_id_perf~",
                table: "instrument_verifications",
                columns: new[] { "organization_id", "instrument_id", "performed_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_instrument_verifications_profile_version_id",
                table: "instrument_verifications",
                column: "profile_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_instruments_compatibility_basis_profile_version_id",
                table: "instruments",
                column: "compatibility_basis_profile_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_instruments_organization_id_name",
                table: "instruments",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instruments_organization_id_order_source_system_id",
                table: "instruments",
                columns: new[] { "organization_id", "order_source_system_id" });

            migrationBuilder.CreateIndex(
                name: "IX_instruments_organization_id_result_destination_system_id",
                table: "instruments",
                columns: new[] { "organization_id", "result_destination_system_id" });

            migrationBuilder.CreateIndex(
                name: "IX_instruments_organization_id_status",
                table: "instruments",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_instruments_organization_id_verification_status",
                table: "instruments",
                columns: new[] { "organization_id", "verification_status" });

            migrationBuilder.CreateIndex(
                name: "IX_interface_profile_versions_profile_id_version",
                table: "interface_profile_versions",
                columns: new[] { "profile_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_interface_profiles_code",
                table: "interface_profiles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_observation_organization_id_conversion_rule_id",
                table: "lab_observation",
                columns: new[] { "organization_id", "conversion_rule_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_observation_organization_id_order_line_id",
                table: "lab_observation",
                columns: new[] { "organization_id", "order_line_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_observation_organization_id_result_id",
                table: "lab_observation",
                columns: new[] { "organization_id", "result_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_observation_result_id_line_number",
                table: "lab_observation",
                columns: new[] { "result_id", "line_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_correlation_id",
                table: "lab_order",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_normalized_message_id",
                table: "lab_order",
                column: "normalized_message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_accession_number",
                table: "lab_order",
                columns: new[] { "organization_id", "accession_number" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_conflicts_with_order_id",
                table: "lab_order",
                columns: new[] { "organization_id", "conflicts_with_order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_mapping_run_id",
                table: "lab_order",
                columns: new[] { "organization_id", "mapping_run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_normalized_message_id",
                table: "lab_order",
                columns: new[] { "organization_id", "normalized_message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_route_id",
                table: "lab_order",
                columns: new[] { "organization_id", "route_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_source_connector_id",
                table: "lab_order",
                columns: new[] { "organization_id", "source_connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_source_message_id",
                table: "lab_order",
                columns: new[] { "organization_id", "source_message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_specimen_id",
                table: "lab_order",
                columns: new[] { "organization_id", "specimen_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_status_created_at",
                table: "lab_order",
                columns: new[] { "organization_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_organization_id_target_instrument_id_status",
                table: "lab_order",
                columns: new[] { "organization_id", "target_instrument_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_lab_order_placer",
                table: "lab_order",
                columns: new[] { "organization_id", "source_external_system_id", "placer_order_id" },
                unique: true,
                filter: "placer_order_id IS NOT NULL AND status NOT IN ('Quarantined','Rejected')");

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_line_order_id_line_number",
                table: "lab_order_line",
                columns: new[] { "order_id", "line_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_line_organization_id_order_id",
                table: "lab_order_line",
                columns: new[] { "organization_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_status_history_order_id_changed_at",
                table: "lab_order_status_history",
                columns: new[] { "order_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_status_history_organization_id_order_id",
                table: "lab_order_status_history",
                columns: new[] { "organization_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_correlation_id",
                table: "lab_result",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_normalized_message_id",
                table: "lab_result",
                column: "normalized_message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_accession_number",
                table: "lab_result",
                columns: new[] { "organization_id", "accession_number" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_correlation_status_created_at",
                table: "lab_result",
                columns: new[] { "organization_id", "correlation_status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_destination_external_system_id",
                table: "lab_result",
                columns: new[] { "organization_id", "destination_external_system_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_mapping_run_id",
                table: "lab_result",
                columns: new[] { "organization_id", "mapping_run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_normalized_message_id",
                table: "lab_result",
                columns: new[] { "organization_id", "normalized_message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_order_id",
                table: "lab_result",
                columns: new[] { "organization_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_placer_order_id",
                table: "lab_result",
                columns: new[] { "organization_id", "placer_order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_route_id",
                table: "lab_result",
                columns: new[] { "organization_id", "route_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_source_connector_id",
                table: "lab_result",
                columns: new[] { "organization_id", "source_connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_source_instrument_id",
                table: "lab_result",
                columns: new[] { "organization_id", "source_instrument_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_source_message_id",
                table: "lab_result",
                columns: new[] { "organization_id", "source_message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_specimen_id",
                table: "lab_result",
                columns: new[] { "organization_id", "specimen_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_organization_id_status_created_at",
                table: "lab_result",
                columns: new[] { "organization_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_status_history_organization_id_result_id",
                table: "lab_result_status_history",
                columns: new[] { "organization_id", "result_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_status_history_result_id_changed_at",
                table: "lab_result_status_history",
                columns: new[] { "result_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_entries_organization_id_concept_id",
                table: "mapping_entries",
                columns: new[] { "organization_id", "concept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_entries_organization_id_target_unit_concept_id",
                table: "mapping_entries",
                columns: new[] { "organization_id", "target_unit_concept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_entries_organization_id_version_id",
                table: "mapping_entries",
                columns: new[] { "organization_id", "version_id" });

            migrationBuilder.CreateIndex(
                name: "ux_mapping_entries_code",
                table: "mapping_entries",
                columns: new[] { "version_id", "external_code" },
                unique: true,
                filter: "direction = 'ToCanonical' AND external_code_system IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_mapping_entries_code_system",
                table: "mapping_entries",
                columns: new[] { "version_id", "external_code", "external_code_system" },
                unique: true,
                filter: "direction = 'ToCanonical' AND external_code_system IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_mapping_entries_concept",
                table: "mapping_entries",
                columns: new[] { "version_id", "concept_id" },
                unique: true,
                filter: "direction = 'FromCanonical'");

            migrationBuilder.CreateIndex(
                name: "IX_mapping_gap_subjects_organization_id_gap_id",
                table: "mapping_gap_subjects",
                columns: new[] { "organization_id", "gap_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_gap_subjects_organization_id_run_id",
                table: "mapping_gap_subjects",
                columns: new[] { "organization_id", "run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_gap_subjects_subject_type_subject_id",
                table: "mapping_gap_subjects",
                columns: new[] { "subject_type", "subject_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_gaps_organization_id_mapping_set_id",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "mapping_set_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_gaps_organization_id_resolved_by_version_id",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "resolved_by_version_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_gaps_organization_id_status_last_seen_at",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "status", "last_seen_at" });

            migrationBuilder.CreateIndex(
                name: "ux_mapping_gaps_code",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "counterparty_id", "direction", "domain", "gap_kind", "code" },
                unique: true,
                filter: "status IN ('Open','Ignored') AND code IS NOT NULL AND code_system IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_mapping_gaps_code_system",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "counterparty_id", "direction", "domain", "gap_kind", "code", "code_system" },
                unique: true,
                filter: "status IN ('Open','Ignored') AND code IS NOT NULL AND code_system IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_mapping_gaps_none",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "counterparty_id", "direction", "domain", "gap_kind" },
                unique: true,
                filter: "status IN ('Open','Ignored') AND code IS NULL AND code_system IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_mapping_gaps_system",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "counterparty_id", "direction", "domain", "gap_kind", "code_system" },
                unique: true,
                filter: "status IN ('Open','Ignored') AND code IS NULL AND code_system IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_mapping_resolutions_organization_id_conversion_rule_id",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "conversion_rule_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_resolutions_organization_id_resolved_concept_id",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "resolved_concept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_resolutions_organization_id_run_id",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_resolutions_organization_id_source_version_id",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "source_version_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_resolutions_organization_id_target_version_id",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "target_version_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_resolutions_run_id_line_number",
                table: "mapping_resolutions",
                columns: new[] { "run_id", "line_number" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_runs_organization_id_request_id",
                table: "mapping_runs",
                columns: new[] { "organization_id", "request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mapping_runs_organization_id_route_id",
                table: "mapping_runs",
                columns: new[] { "organization_id", "route_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_runs_organization_id_subject_type_subject_id",
                table: "mapping_runs",
                columns: new[] { "organization_id", "subject_type", "subject_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_set_versions_mapping_set_id_version_number",
                table: "mapping_set_versions",
                columns: new[] { "mapping_set_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mapping_set_versions_organization_id_copied_from_version_id",
                table: "mapping_set_versions",
                columns: new[] { "organization_id", "copied_from_version_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mapping_set_versions_organization_id_mapping_set_id",
                table: "mapping_set_versions",
                columns: new[] { "organization_id", "mapping_set_id" });

            migrationBuilder.CreateIndex(
                name: "ux_mapping_set_versions_draft",
                table: "mapping_set_versions",
                column: "mapping_set_id",
                unique: true,
                filter: "status = 'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_mapping_sets_organization_id_active_version_id",
                table: "mapping_sets",
                columns: new[] { "organization_id", "active_version_id" });

            migrationBuilder.CreateIndex(
                name: "ux_mapping_sets_external_system",
                table: "mapping_sets",
                columns: new[] { "organization_id", "external_system_id", "direction", "domain" },
                unique: true,
                filter: "external_system_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_mapping_sets_instrument",
                table: "mapping_sets",
                columns: new[] { "organization_id", "instrument_id", "direction", "domain" },
                unique: true,
                filter: "instrument_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_metric_samples_bucket_start",
                table: "metric_samples",
                column: "bucket_start");

            migrationBuilder.CreateIndex(
                name: "ux_metric_samples_key",
                table: "metric_samples",
                columns: new[] { "organization_id", "metric_name", "dimensions_key", "bucket_start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_normalized_handoffs_consumer_status_available_at_receipt_se~",
                table: "normalized_handoffs",
                columns: new[] { "consumer", "status", "available_at", "receipt_sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_normalized_handoffs_lease_expires_at",
                table: "normalized_handoffs",
                column: "lease_expires_at",
                filter: "status = 'Claimed'");

            migrationBuilder.CreateIndex(
                name: "IX_normalized_handoffs_normalized_message_id",
                table: "normalized_handoffs",
                column: "normalized_message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_normalized_handoffs_organization_id_normalized_message_id",
                table: "normalized_handoffs",
                columns: new[] { "organization_id", "normalized_message_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_normalized_handoffs_pending",
                table: "normalized_handoffs",
                columns: new[] { "consumer", "receipt_sequence" },
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_normalized_messages_message_id_version",
                table: "normalized_messages",
                columns: new[] { "message_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_normalized_messages_organization_id_created_at",
                table: "normalized_messages",
                columns: new[] { "organization_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_normalized_messages_organization_id_message_id_run_id",
                table: "normalized_messages",
                columns: new[] { "organization_id", "message_id", "run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_normalized_messages_run_id",
                table: "normalized_messages",
                column: "run_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_normalized_messages_current",
                table: "normalized_messages",
                column: "message_id",
                unique: true,
                filter: "is_current");

            migrationBuilder.CreateIndex(
                name: "IX_organization_settings_organization_id_setting_definition_id",
                table: "organization_settings",
                columns: new[] { "organization_id", "setting_definition_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_settings_setting_definition_id",
                table: "organization_settings",
                column: "setting_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_status_history_organization_id_changed_at",
                table: "organization_status_history",
                columns: new[] { "organization_id", "changed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_organization_status_history_organization_id_status_version",
                table: "organization_status_history",
                columns: new[] { "organization_id", "status_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_at",
                table: "organizations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_display_name",
                table: "organizations",
                column: "display_name");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_status",
                table: "organizations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_organizations_code",
                table: "organizations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parser_types_code_version",
                table: "parser_types",
                columns: new[] { "code", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_limits_limit_key_kind_is_message_volume",
                table: "plan_limits",
                columns: new[] { "limit_key", "kind", "is_message_volume" });

            migrationBuilder.CreateIndex(
                name: "IX_plan_limits_plan_id_limit_key",
                table: "plan_limits",
                columns: new[] { "plan_id", "limit_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plans_code_version",
                table: "plans",
                columns: new[] { "code", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processing_reviews_message_id_decided_at",
                table: "processing_reviews",
                columns: new[] { "message_id", "decided_at" });

            migrationBuilder.CreateIndex(
                name: "IX_processing_reviews_organization_id_message_id_run_id",
                table: "processing_reviews",
                columns: new[] { "organization_id", "message_id", "run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_processing_runs_binding_id",
                table: "processing_runs",
                column: "binding_id");

            migrationBuilder.CreateIndex(
                name: "IX_processing_runs_message_id_attempt_number",
                table: "processing_runs",
                columns: new[] { "message_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processing_runs_parser_type_id",
                table: "processing_runs",
                column: "parser_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_processing_runs_profile_version_id",
                table: "processing_runs",
                column: "profile_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_processing_runs_status_next_attempt_at",
                table: "processing_runs",
                columns: new[] { "status", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "ux_processing_runs_open_per_message",
                table: "processing_runs",
                column: "message_id",
                unique: true,
                filter: "status IN ('Scheduled','InProgress')");

            migrationBuilder.CreateIndex(
                name: "IX_profile_parser_bindings_parser_type_id",
                table: "profile_parser_bindings",
                column: "parser_type_id");

            migrationBuilder.CreateIndex(
                name: "ux_profile_parser_bindings_active",
                table: "profile_parser_bindings",
                column: "profile_version_id",
                unique: true,
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_raw_payloads_organization_id_stored_at",
                table: "raw_payloads",
                columns: new[] { "organization_id", "stored_at" });

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_cases_organization_id_attempt_id",
                table: "reconciliation_cases",
                columns: new[] { "organization_id", "attempt_id" });

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_cases_organization_id_delivery_id",
                table: "reconciliation_cases",
                columns: new[] { "organization_id", "delivery_id" });

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_cases_organization_id_status_opened_at",
                table: "reconciliation_cases",
                columns: new[] { "organization_id", "status", "opened_at" });

            migrationBuilder.CreateIndex(
                name: "ux_reconciliation_cases_open_attempt",
                table: "reconciliation_cases",
                column: "attempt_id",
                unique: true,
                filter: "status IN ('Open','Escalated')");

            migrationBuilder.CreateIndex(
                name: "ix_report_runs_expiry",
                table: "report_runs",
                column: "expires_at",
                filter: "status = 'Succeeded'");

            migrationBuilder.CreateIndex(
                name: "IX_report_runs_organization_id_created_at",
                table: "report_runs",
                columns: new[] { "organization_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_report_runs_organization_id_request_key",
                table: "report_runs",
                columns: new[] { "organization_id", "request_key" },
                unique: true,
                filter: "request_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_report_runs_report_code",
                table: "report_runs",
                column: "report_code");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_role_id",
                table: "role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ux_roles_normalized_name",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_route_delivery_policies_organization_id_route_id",
                table: "route_delivery_policies",
                columns: new[] { "organization_id", "route_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routes_organization_id_destination_connector_id",
                table: "routes",
                columns: new[] { "organization_id", "destination_connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_routes_organization_id_instrument_id",
                table: "routes",
                columns: new[] { "organization_id", "instrument_id" });

            migrationBuilder.CreateIndex(
                name: "IX_routes_organization_id_name",
                table: "routes",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_routes_enabled_source_kind",
                table: "routes",
                columns: new[] { "organization_id", "source_connector_id", "message_kind" },
                unique: true,
                filter: "status = 'Enabled'");

            migrationBuilder.CreateIndex(
                name: "IX_setting_definitions_key",
                table: "setting_definitions",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulator_behaviors_organization_id_connector_id",
                table: "simulator_behaviors",
                columns: new[] { "organization_id", "connector_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulator_sink_messages_connector_id_received_at",
                table: "simulator_sink_messages",
                columns: new[] { "connector_id", "received_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_simulator_sink_messages_organization_id_attempt_id",
                table: "simulator_sink_messages",
                columns: new[] { "organization_id", "attempt_id" });

            migrationBuilder.CreateIndex(
                name: "IX_simulator_sink_messages_organization_id_connector_id",
                table: "simulator_sink_messages",
                columns: new[] { "organization_id", "connector_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_history_from_plan_id",
                table: "subscription_history",
                column: "from_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_history_organization_id_subscription_id",
                table: "subscription_history",
                columns: new[] { "organization_id", "subscription_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_history_subscription_id_changed_at",
                table: "subscription_history",
                columns: new[] { "subscription_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_history_to_plan_id",
                table: "subscription_history",
                column: "to_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_organization_id",
                table: "subscriptions",
                column: "organization_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_plan_id",
                table: "subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_unit_conversion_rules_organization_id_from_unit_concept_id",
                table: "unit_conversion_rules",
                columns: new[] { "organization_id", "from_unit_concept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_conversion_rules_organization_id_supersedes_rule_id",
                table: "unit_conversion_rules",
                columns: new[] { "organization_id", "supersedes_rule_id" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_conversion_rules_organization_id_to_unit_concept_id",
                table: "unit_conversion_rules",
                columns: new[] { "organization_id", "to_unit_concept_id" });

            migrationBuilder.CreateIndex(
                name: "ux_unit_conversion_rules_approved",
                table: "unit_conversion_rules",
                columns: new[] { "organization_id", "test_concept_id", "from_unit_concept_id", "to_unit_concept_id" },
                unique: true,
                filter: "status = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_usage_counters_organization_id_period_metric_key",
                table: "usage_counters",
                columns: new[] { "organization_id", "period", "metric_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_user_id",
                table: "user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_expires_at",
                table: "user_invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_organization_id",
                table: "user_invitations",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_token_hash",
                table: "user_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_user_id",
                table: "user_invitations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_assignments_organization_id_role_id",
                table: "user_role_assignments",
                columns: new[] { "organization_id", "role_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_role_assignments_role_id",
                table: "user_role_assignments",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_organization_id_created_at",
                table: "users",
                columns: new[] { "organization_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_users_organization_id_display_name",
                table: "users",
                columns: new[] { "organization_id", "display_name" });

            migrationBuilder.CreateIndex(
                name: "IX_users_organization_id_status",
                table: "users",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_normalized_user_name",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_validation_findings_organization_id_message_id",
                table: "validation_findings",
                columns: new[] { "organization_id", "message_id" });

            migrationBuilder.CreateIndex(
                name: "IX_validation_findings_organization_id_message_id_run_id",
                table: "validation_findings",
                columns: new[] { "organization_id", "message_id", "run_id" });

            migrationBuilder.CreateIndex(
                name: "IX_validation_findings_run_id",
                table: "validation_findings",
                column: "run_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_entries_mapping_set_versions_organization_id_versio~",
                table: "mapping_entries",
                columns: new[] { "organization_id", "version_id" },
                principalTable: "mapping_set_versions",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_gap_subjects_mapping_gaps_organization_id_gap_id",
                table: "mapping_gap_subjects",
                columns: new[] { "organization_id", "gap_id" },
                principalTable: "mapping_gaps",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_gaps_mapping_set_versions_organization_id_resolved_~",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "resolved_by_version_id" },
                principalTable: "mapping_set_versions",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_gaps_mapping_sets_organization_id_mapping_set_id",
                table: "mapping_gaps",
                columns: new[] { "organization_id", "mapping_set_id" },
                principalTable: "mapping_sets",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_resolutions_mapping_set_versions_organization_id_so~",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "source_version_id" },
                principalTable: "mapping_set_versions",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_resolutions_mapping_set_versions_organization_id_ta~",
                table: "mapping_resolutions",
                columns: new[] { "organization_id", "target_version_id" },
                principalTable: "mapping_set_versions",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mapping_set_versions_mapping_sets_organization_id_mapping_s~",
                table: "mapping_set_versions",
                columns: new[] { "organization_id", "mapping_set_id" },
                principalTable: "mapping_sets",
                principalColumns: new[] { "organization_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_external_systems_organizations_organization_id",
                table: "external_systems");

            migrationBuilder.DropForeignKey(
                name: "FK_instruments_organizations_organization_id",
                table: "instruments");

            migrationBuilder.DropForeignKey(
                name: "FK_mapping_set_versions_organizations_organization_id",
                table: "mapping_set_versions");

            migrationBuilder.DropForeignKey(
                name: "FK_mapping_sets_organizations_organization_id",
                table: "mapping_sets");

            migrationBuilder.DropForeignKey(
                name: "FK_instruments_external_systems_organization_id_order_source_s~",
                table: "instruments");

            migrationBuilder.DropForeignKey(
                name: "FK_instruments_external_systems_organization_id_result_destina~",
                table: "instruments");

            migrationBuilder.DropForeignKey(
                name: "FK_mapping_sets_external_systems_organization_id_external_syst~",
                table: "mapping_sets");

            migrationBuilder.DropForeignKey(
                name: "FK_mapping_sets_instruments_organization_id_instrument_id",
                table: "mapping_sets");

            migrationBuilder.DropForeignKey(
                name: "FK_mapping_sets_mapping_set_versions_organization_id_active_ve~",
                table: "mapping_sets");

            migrationBuilder.DropTable(
                name: "acknowledgment_records");

            migrationBuilder.DropTable(
                name: "alert_rule_settings");

            migrationBuilder.DropTable(
                name: "alert_state_history");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "audit_integrity_checks");

            migrationBuilder.DropTable(
                name: "audit_spool_items");

            migrationBuilder.DropTable(
                name: "connector_config_revisions");

            migrationBuilder.DropTable(
                name: "connector_health_snapshots");

            migrationBuilder.DropTable(
                name: "connector_health_states");

            migrationBuilder.DropTable(
                name: "connector_secret_refs");

            migrationBuilder.DropTable(
                name: "connector_test_results");

            migrationBuilder.DropTable(
                name: "daily_facts");

            migrationBuilder.DropTable(
                name: "deduplication_keys");

            migrationBuilder.DropTable(
                name: "external_identities");

            migrationBuilder.DropTable(
                name: "idempotency_records");

            migrationBuilder.DropTable(
                name: "inbound_message_status_history");

            migrationBuilder.DropTable(
                name: "inbound_processing_handoffs");

            migrationBuilder.DropTable(
                name: "inbound_refusals");

            migrationBuilder.DropTable(
                name: "instrument_verifications");

            migrationBuilder.DropTable(
                name: "lab_observation");

            migrationBuilder.DropTable(
                name: "lab_order_status_history");

            migrationBuilder.DropTable(
                name: "lab_result_status_history");

            migrationBuilder.DropTable(
                name: "mapping_entries");

            migrationBuilder.DropTable(
                name: "mapping_gap_subjects");

            migrationBuilder.DropTable(
                name: "mapping_resolutions");

            migrationBuilder.DropTable(
                name: "metric_samples");

            migrationBuilder.DropTable(
                name: "normalized_handoffs");

            migrationBuilder.DropTable(
                name: "organization_settings");

            migrationBuilder.DropTable(
                name: "organization_status_history");

            migrationBuilder.DropTable(
                name: "plan_limits");

            migrationBuilder.DropTable(
                name: "processing_reviews");

            migrationBuilder.DropTable(
                name: "reconciliation_cases");

            migrationBuilder.DropTable(
                name: "report_runs");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "route_delivery_policies");

            migrationBuilder.DropTable(
                name: "simulator_behaviors");

            migrationBuilder.DropTable(
                name: "simulator_sink_messages");

            migrationBuilder.DropTable(
                name: "subscription_history");

            migrationBuilder.DropTable(
                name: "usage_counters");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_invitations");

            migrationBuilder.DropTable(
                name: "user_role_assignments");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "validation_findings");

            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "audit_action_definitions");

            migrationBuilder.DropTable(
                name: "audit_seals");

            migrationBuilder.DropTable(
                name: "lab_order_line");

            migrationBuilder.DropTable(
                name: "lab_result");

            migrationBuilder.DropTable(
                name: "mapping_gaps");

            migrationBuilder.DropTable(
                name: "unit_conversion_rules");

            migrationBuilder.DropTable(
                name: "setting_definitions");

            migrationBuilder.DropTable(
                name: "limit_definitions");

            migrationBuilder.DropTable(
                name: "report_definitions");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "delivery_attempts");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "alert_rules");

            migrationBuilder.DropTable(
                name: "lab_order");

            migrationBuilder.DropTable(
                name: "canonical_concepts");

            migrationBuilder.DropTable(
                name: "delivery_items");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "mapping_runs");

            migrationBuilder.DropTable(
                name: "normalized_messages");

            migrationBuilder.DropTable(
                name: "delivery_requests");

            migrationBuilder.DropTable(
                name: "processing_runs");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "inbound_messages");

            migrationBuilder.DropTable(
                name: "profile_parser_bindings");

            migrationBuilder.DropTable(
                name: "connectors");

            migrationBuilder.DropTable(
                name: "raw_payloads");

            migrationBuilder.DropTable(
                name: "parser_types");

            migrationBuilder.DropTable(
                name: "adapter_types");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "external_systems");

            migrationBuilder.DropTable(
                name: "instruments");

            migrationBuilder.DropTable(
                name: "interface_profile_versions");

            migrationBuilder.DropTable(
                name: "interface_profiles");

            migrationBuilder.DropTable(
                name: "mapping_set_versions");

            migrationBuilder.DropTable(
                name: "mapping_sets");

            migrationBuilder.DropSequence(
                name: "inbound_message_receipt_seq");
        }
    }
}
