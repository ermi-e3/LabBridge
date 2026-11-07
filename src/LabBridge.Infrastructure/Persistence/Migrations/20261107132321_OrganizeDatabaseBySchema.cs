using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganizeDatabaseBySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "delivery_management");

            migrationBuilder.EnsureSchema(
                name: "instrument_management");

            migrationBuilder.EnsureSchema(
                name: "monitoring");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "testCode_mapping");

            migrationBuilder.EnsureSchema(
                name: "reporting");

            migrationBuilder.EnsureSchema(
                name: "message_ingestion");

            migrationBuilder.EnsureSchema(
                name: "orderResult_management");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "subscription_management");

            migrationBuilder.EnsureSchema(
                name: "message_processing");

            migrationBuilder.EnsureSchema(
                name: "organization_management");

            migrationBuilder.EnsureSchema(
                name: "ingestion");

            migrationBuilder.RenameTable(
                name: "validation_findings",
                newName: "validation_findings",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "user_tokens",
                newName: "user_tokens",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "user_role_assignments",
                newName: "user_role_assignments",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "user_invitations",
                newName: "user_invitations",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "user_claims",
                newName: "user_claims",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "usage_counters",
                newName: "usage_counters",
                newSchema: "subscription_management");

            migrationBuilder.RenameTable(
                name: "unit_conversion_rules",
                newName: "unit_conversion_rules",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "subscriptions",
                newName: "subscriptions",
                newSchema: "subscription_management");

            migrationBuilder.RenameTable(
                name: "subscription_history",
                newName: "subscription_history",
                newSchema: "subscription_management");

            migrationBuilder.RenameTable(
                name: "simulator_sink_messages",
                newName: "simulator_sink_messages",
                newSchema: "delivery_management");

            migrationBuilder.RenameTable(
                name: "simulator_behaviors",
                newName: "simulator_behaviors",
                newSchema: "delivery_management");

            migrationBuilder.RenameTable(
                name: "setting_definitions",
                newName: "setting_definitions",
                newSchema: "organization_management");

            migrationBuilder.RenameTable(
                name: "routes",
                newName: "routes",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "route_delivery_policies",
                newName: "route_delivery_policies",
                newSchema: "delivery_management");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "roles",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "role_permissions",
                newName: "role_permissions",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "role_claims",
                newName: "role_claims",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "report_runs",
                newName: "report_runs",
                newSchema: "reporting");

            migrationBuilder.RenameTable(
                name: "report_definitions",
                newName: "report_definitions",
                newSchema: "reporting");

            migrationBuilder.RenameTable(
                name: "reconciliation_cases",
                newName: "reconciliation_cases",
                newSchema: "delivery_management");

            migrationBuilder.RenameTable(
                name: "raw_payloads",
                newName: "raw_payloads",
                newSchema: "message_ingestion");

            migrationBuilder.RenameTable(
                name: "profile_parser_bindings",
                newName: "profile_parser_bindings",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "processing_runs",
                newName: "processing_runs",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "processing_reviews",
                newName: "processing_reviews",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "plans",
                newName: "plans",
                newSchema: "subscription_management");

            migrationBuilder.RenameTable(
                name: "plan_limits",
                newName: "plan_limits",
                newSchema: "subscription_management");

            migrationBuilder.RenameTable(
                name: "permissions",
                newName: "permissions",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "parser_types",
                newName: "parser_types",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "organizations",
                newName: "organizations",
                newSchema: "organization_management");

            migrationBuilder.RenameTable(
                name: "organization_status_history",
                newName: "organization_status_history",
                newSchema: "organization_management");

            migrationBuilder.RenameTable(
                name: "organization_settings",
                newName: "organization_settings",
                newSchema: "organization_management");

            migrationBuilder.RenameTable(
                name: "normalized_messages",
                newName: "normalized_messages",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "normalized_handoffs",
                newName: "normalized_handoffs",
                newSchema: "message_processing");

            migrationBuilder.RenameTable(
                name: "metric_samples",
                newName: "metric_samples",
                newSchema: "monitoring");

            migrationBuilder.RenameTable(
                name: "mapping_sets",
                newName: "mapping_sets",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "mapping_set_versions",
                newName: "mapping_set_versions",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "mapping_runs",
                newName: "mapping_runs",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "mapping_resolutions",
                newName: "mapping_resolutions",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "mapping_gaps",
                newName: "mapping_gaps",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "mapping_gap_subjects",
                newName: "mapping_gap_subjects",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "mapping_entries",
                newName: "mapping_entries",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "limit_definitions",
                newName: "limit_definitions",
                newSchema: "subscription_management");

            migrationBuilder.RenameTable(
                name: "lab_result_status_history",
                newName: "lab_result_status_history",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "lab_result",
                newName: "lab_result",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "lab_order_status_history",
                newName: "lab_order_status_history",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "lab_order_line",
                newName: "lab_order_line",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "lab_order",
                newName: "lab_order",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "lab_observation",
                newName: "lab_observation",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "interface_profiles",
                newName: "interface_profiles",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "interface_profile_versions",
                newName: "interface_profile_versions",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "instruments",
                newName: "instruments",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "instrument_verifications",
                newName: "instrument_verifications",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "inbound_refusals",
                newName: "inbound_refusals",
                newSchema: "message_ingestion");

            migrationBuilder.RenameTable(
                name: "inbound_processing_handoffs",
                newName: "inbound_processing_handoffs",
                newSchema: "message_ingestion");

            migrationBuilder.RenameTable(
                name: "inbound_messages",
                newName: "inbound_messages",
                newSchema: "message_ingestion");

            migrationBuilder.RenameTable(
                name: "inbound_message_status_history",
                newName: "inbound_message_status_history",
                newSchema: "message_ingestion");

            migrationBuilder.RenameTable(
                name: "external_systems",
                newName: "external_systems",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "external_identities",
                newName: "external_identities",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "delivery_requests",
                newName: "delivery_requests",
                newSchema: "orderResult_management");

            migrationBuilder.RenameTable(
                name: "delivery_items",
                newName: "delivery_items",
                newSchema: "delivery_management");

            migrationBuilder.RenameTable(
                name: "delivery_attempts",
                newName: "delivery_attempts",
                newSchema: "delivery_management");

            migrationBuilder.RenameTable(
                name: "deduplication_keys",
                newName: "deduplication_keys",
                newSchema: "message_ingestion");

            migrationBuilder.RenameTable(
                name: "daily_facts",
                newName: "daily_facts",
                newSchema: "reporting");

            migrationBuilder.RenameTable(
                name: "connectors",
                newName: "connectors",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "connector_test_results",
                newName: "connector_test_results",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "connector_secret_refs",
                newName: "connector_secret_refs",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "connector_health_states",
                newName: "connector_health_states",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "connector_health_snapshots",
                newName: "connector_health_snapshots",
                newSchema: "monitoring");

            migrationBuilder.RenameTable(
                name: "connector_config_revisions",
                newName: "connector_config_revisions",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "canonical_concepts",
                newName: "canonical_concepts",
                newSchema: "testCode_mapping");

            migrationBuilder.RenameTable(
                name: "audit_spool_items",
                newName: "audit_spool_items",
                newSchema: "audit");

            migrationBuilder.RenameTable(
                name: "audit_events",
                newName: "audit_events",
                newSchema: "audit");

            migrationBuilder.RenameTable(
                name: "alerts",
                newName: "alerts",
                newSchema: "monitoring");

            migrationBuilder.RenameTable(
                name: "alert_state_history",
                newName: "alert_state_history",
                newSchema: "monitoring");

            migrationBuilder.RenameTable(
                name: "alert_rules",
                newName: "alert_rules",
                newSchema: "monitoring");

            migrationBuilder.RenameTable(
                name: "alert_rule_settings",
                newName: "alert_rule_settings",
                newSchema: "monitoring");

            migrationBuilder.RenameTable(
                name: "adapter_types",
                newName: "adapter_types",
                newSchema: "instrument_management");

            migrationBuilder.RenameTable(
                name: "acknowledgment_records",
                newName: "acknowledgment_records",
                newSchema: "delivery_management");

            migrationBuilder.RenameSequence(
                name: "inbound_message_receipt_seq",
                newName: "inbound_message_receipt_seq",
                newSchema: "ingestion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "validation_findings",
                schema: "message_processing",
                newName: "validation_findings");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "identity",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "user_tokens",
                schema: "identity",
                newName: "user_tokens");

            migrationBuilder.RenameTable(
                name: "user_role_assignments",
                schema: "identity",
                newName: "user_role_assignments");

            migrationBuilder.RenameTable(
                name: "user_invitations",
                schema: "identity",
                newName: "user_invitations");

            migrationBuilder.RenameTable(
                name: "user_claims",
                schema: "identity",
                newName: "user_claims");

            migrationBuilder.RenameTable(
                name: "usage_counters",
                schema: "subscription_management",
                newName: "usage_counters");

            migrationBuilder.RenameTable(
                name: "unit_conversion_rules",
                schema: "testCode_mapping",
                newName: "unit_conversion_rules");

            migrationBuilder.RenameTable(
                name: "subscriptions",
                schema: "subscription_management",
                newName: "subscriptions");

            migrationBuilder.RenameTable(
                name: "subscription_history",
                schema: "subscription_management",
                newName: "subscription_history");

            migrationBuilder.RenameTable(
                name: "simulator_sink_messages",
                schema: "delivery_management",
                newName: "simulator_sink_messages");

            migrationBuilder.RenameTable(
                name: "simulator_behaviors",
                schema: "delivery_management",
                newName: "simulator_behaviors");

            migrationBuilder.RenameTable(
                name: "setting_definitions",
                schema: "organization_management",
                newName: "setting_definitions");

            migrationBuilder.RenameTable(
                name: "routes",
                schema: "instrument_management",
                newName: "routes");

            migrationBuilder.RenameTable(
                name: "route_delivery_policies",
                schema: "delivery_management",
                newName: "route_delivery_policies");

            migrationBuilder.RenameTable(
                name: "roles",
                schema: "identity",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "role_permissions",
                schema: "identity",
                newName: "role_permissions");

            migrationBuilder.RenameTable(
                name: "role_claims",
                schema: "identity",
                newName: "role_claims");

            migrationBuilder.RenameTable(
                name: "report_runs",
                schema: "reporting",
                newName: "report_runs");

            migrationBuilder.RenameTable(
                name: "report_definitions",
                schema: "reporting",
                newName: "report_definitions");

            migrationBuilder.RenameTable(
                name: "reconciliation_cases",
                schema: "delivery_management",
                newName: "reconciliation_cases");

            migrationBuilder.RenameTable(
                name: "raw_payloads",
                schema: "message_ingestion",
                newName: "raw_payloads");

            migrationBuilder.RenameTable(
                name: "profile_parser_bindings",
                schema: "message_processing",
                newName: "profile_parser_bindings");

            migrationBuilder.RenameTable(
                name: "processing_runs",
                schema: "message_processing",
                newName: "processing_runs");

            migrationBuilder.RenameTable(
                name: "processing_reviews",
                schema: "message_processing",
                newName: "processing_reviews");

            migrationBuilder.RenameTable(
                name: "plans",
                schema: "subscription_management",
                newName: "plans");

            migrationBuilder.RenameTable(
                name: "plan_limits",
                schema: "subscription_management",
                newName: "plan_limits");

            migrationBuilder.RenameTable(
                name: "permissions",
                schema: "identity",
                newName: "permissions");

            migrationBuilder.RenameTable(
                name: "parser_types",
                schema: "message_processing",
                newName: "parser_types");

            migrationBuilder.RenameTable(
                name: "organizations",
                schema: "organization_management",
                newName: "organizations");

            migrationBuilder.RenameTable(
                name: "organization_status_history",
                schema: "organization_management",
                newName: "organization_status_history");

            migrationBuilder.RenameTable(
                name: "organization_settings",
                schema: "organization_management",
                newName: "organization_settings");

            migrationBuilder.RenameTable(
                name: "normalized_messages",
                schema: "message_processing",
                newName: "normalized_messages");

            migrationBuilder.RenameTable(
                name: "normalized_handoffs",
                schema: "message_processing",
                newName: "normalized_handoffs");

            migrationBuilder.RenameTable(
                name: "metric_samples",
                schema: "monitoring",
                newName: "metric_samples");

            migrationBuilder.RenameTable(
                name: "mapping_sets",
                schema: "testCode_mapping",
                newName: "mapping_sets");

            migrationBuilder.RenameTable(
                name: "mapping_set_versions",
                schema: "testCode_mapping",
                newName: "mapping_set_versions");

            migrationBuilder.RenameTable(
                name: "mapping_runs",
                schema: "testCode_mapping",
                newName: "mapping_runs");

            migrationBuilder.RenameTable(
                name: "mapping_resolutions",
                schema: "testCode_mapping",
                newName: "mapping_resolutions");

            migrationBuilder.RenameTable(
                name: "mapping_gaps",
                schema: "testCode_mapping",
                newName: "mapping_gaps");

            migrationBuilder.RenameTable(
                name: "mapping_gap_subjects",
                schema: "testCode_mapping",
                newName: "mapping_gap_subjects");

            migrationBuilder.RenameTable(
                name: "mapping_entries",
                schema: "testCode_mapping",
                newName: "mapping_entries");

            migrationBuilder.RenameTable(
                name: "limit_definitions",
                schema: "subscription_management",
                newName: "limit_definitions");

            migrationBuilder.RenameTable(
                name: "lab_result_status_history",
                schema: "orderResult_management",
                newName: "lab_result_status_history");

            migrationBuilder.RenameTable(
                name: "lab_result",
                schema: "orderResult_management",
                newName: "lab_result");

            migrationBuilder.RenameTable(
                name: "lab_order_status_history",
                schema: "orderResult_management",
                newName: "lab_order_status_history");

            migrationBuilder.RenameTable(
                name: "lab_order_line",
                schema: "orderResult_management",
                newName: "lab_order_line");

            migrationBuilder.RenameTable(
                name: "lab_order",
                schema: "orderResult_management",
                newName: "lab_order");

            migrationBuilder.RenameTable(
                name: "lab_observation",
                schema: "orderResult_management",
                newName: "lab_observation");

            migrationBuilder.RenameTable(
                name: "interface_profiles",
                schema: "instrument_management",
                newName: "interface_profiles");

            migrationBuilder.RenameTable(
                name: "interface_profile_versions",
                schema: "instrument_management",
                newName: "interface_profile_versions");

            migrationBuilder.RenameTable(
                name: "instruments",
                schema: "instrument_management",
                newName: "instruments");

            migrationBuilder.RenameTable(
                name: "instrument_verifications",
                schema: "instrument_management",
                newName: "instrument_verifications");

            migrationBuilder.RenameTable(
                name: "inbound_refusals",
                schema: "message_ingestion",
                newName: "inbound_refusals");

            migrationBuilder.RenameTable(
                name: "inbound_processing_handoffs",
                schema: "message_ingestion",
                newName: "inbound_processing_handoffs");

            migrationBuilder.RenameTable(
                name: "inbound_messages",
                schema: "message_ingestion",
                newName: "inbound_messages");

            migrationBuilder.RenameTable(
                name: "inbound_message_status_history",
                schema: "message_ingestion",
                newName: "inbound_message_status_history");

            migrationBuilder.RenameTable(
                name: "external_systems",
                schema: "instrument_management",
                newName: "external_systems");

            migrationBuilder.RenameTable(
                name: "external_identities",
                schema: "identity",
                newName: "external_identities");

            migrationBuilder.RenameTable(
                name: "delivery_requests",
                schema: "orderResult_management",
                newName: "delivery_requests");

            migrationBuilder.RenameTable(
                name: "delivery_items",
                schema: "delivery_management",
                newName: "delivery_items");

            migrationBuilder.RenameTable(
                name: "delivery_attempts",
                schema: "delivery_management",
                newName: "delivery_attempts");

            migrationBuilder.RenameTable(
                name: "deduplication_keys",
                schema: "message_ingestion",
                newName: "deduplication_keys");

            migrationBuilder.RenameTable(
                name: "daily_facts",
                schema: "reporting",
                newName: "daily_facts");

            migrationBuilder.RenameTable(
                name: "connectors",
                schema: "instrument_management",
                newName: "connectors");

            migrationBuilder.RenameTable(
                name: "connector_test_results",
                schema: "instrument_management",
                newName: "connector_test_results");

            migrationBuilder.RenameTable(
                name: "connector_secret_refs",
                schema: "instrument_management",
                newName: "connector_secret_refs");

            migrationBuilder.RenameTable(
                name: "connector_health_states",
                schema: "instrument_management",
                newName: "connector_health_states");

            migrationBuilder.RenameTable(
                name: "connector_health_snapshots",
                schema: "monitoring",
                newName: "connector_health_snapshots");

            migrationBuilder.RenameTable(
                name: "connector_config_revisions",
                schema: "instrument_management",
                newName: "connector_config_revisions");

            migrationBuilder.RenameTable(
                name: "canonical_concepts",
                schema: "testCode_mapping",
                newName: "canonical_concepts");

            migrationBuilder.RenameTable(
                name: "audit_spool_items",
                schema: "audit",
                newName: "audit_spool_items");

            migrationBuilder.RenameTable(
                name: "audit_events",
                schema: "audit",
                newName: "audit_events");

            migrationBuilder.RenameTable(
                name: "alerts",
                schema: "monitoring",
                newName: "alerts");

            migrationBuilder.RenameTable(
                name: "alert_state_history",
                schema: "monitoring",
                newName: "alert_state_history");

            migrationBuilder.RenameTable(
                name: "alert_rules",
                schema: "monitoring",
                newName: "alert_rules");

            migrationBuilder.RenameTable(
                name: "alert_rule_settings",
                schema: "monitoring",
                newName: "alert_rule_settings");

            migrationBuilder.RenameTable(
                name: "adapter_types",
                schema: "instrument_management",
                newName: "adapter_types");

            migrationBuilder.RenameTable(
                name: "acknowledgment_records",
                schema: "delivery_management",
                newName: "acknowledgment_records");

            migrationBuilder.RenameSequence(
                name: "inbound_message_receipt_seq",
                schema: "ingestion",
                newName: "inbound_message_receipt_seq");
        }
    }
}
