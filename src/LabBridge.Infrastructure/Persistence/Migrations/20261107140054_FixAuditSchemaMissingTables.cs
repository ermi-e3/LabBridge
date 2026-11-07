using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditSchemaMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.RenameTable(
                name: "idempotency_records",
                newName: "idempotency_records",
                newSchema: "common");

            migrationBuilder.RenameTable(
                name: "audit_seals",
                newName: "audit_seals",
                newSchema: "audit");

            migrationBuilder.RenameTable(
                name: "audit_integrity_checks",
                newName: "audit_integrity_checks",
                newSchema: "audit");

            migrationBuilder.RenameTable(
                name: "audit_action_definitions",
                newName: "audit_action_definitions",
                newSchema: "audit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "idempotency_records",
                schema: "common",
                newName: "idempotency_records");

            migrationBuilder.RenameTable(
                name: "audit_seals",
                schema: "audit",
                newName: "audit_seals");

            migrationBuilder.RenameTable(
                name: "audit_integrity_checks",
                schema: "audit",
                newName: "audit_integrity_checks");

            migrationBuilder.RenameTable(
                name: "audit_action_definitions",
                schema: "audit",
                newName: "audit_action_definitions");
        }
    }
}
