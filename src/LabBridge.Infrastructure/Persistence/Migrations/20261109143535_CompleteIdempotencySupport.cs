using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteIdempotencySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_idempotency_org_key",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropIndex(
                name: "ux_idempotency_platform_key",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropCheckConstraint(
                name: "ck_idempotency_records_status",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.RenameIndex(
                name: "IX_idempotency_records_expires_at",
                schema: "common",
                table: "idempotency_records",
                newName: "ix_idempotency_expires_at");

            migrationBuilder.AlterColumn<int>(
                name: "response_status",
                schema: "common",
                table: "idempotency_records",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "fingerprint",
                schema: "common",
                table: "idempotency_records",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "operation_scope",
                schema: "common",
                table: "idempotency_records",
                type: "citext",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "reservation_token",
                schema: "common",
                table: "idempotency_records",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "response_content_type",
                schema: "common",
                table: "idempotency_records",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "response_headers",
                schema: "common",
                table: "idempotency_records",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "common",
                table: "idempotency_records",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ux_idempotency_org_scope_key",
                schema: "common",
                table: "idempotency_records",
                columns: new[] { "organization_id", "operation_scope", "key" },
                unique: true,
                filter: "organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_idempotency_platform_scope_key",
                schema: "common",
                table: "idempotency_records",
                columns: new[] { "operation_scope", "key" },
                unique: true,
                filter: "organization_id IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_idempotency_records_fingerprint_length",
                schema: "common",
                table: "idempotency_records",
                sql: "char_length(fingerprint) BETWEEN 1 AND 128");

            migrationBuilder.AddCheckConstraint(
                name: "ck_idempotency_records_operation_scope",
                schema: "common",
                table: "idempotency_records",
                sql: "char_length(operation_scope) BETWEEN 1 AND 256");

            migrationBuilder.AddCheckConstraint(
                name: "ck_idempotency_records_reservation_token",
                schema: "common",
                table: "idempotency_records",
                sql: "reservation_token <> '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_idempotency_records_response",
                schema: "common",
                table: "idempotency_records",
                sql: "(\r\n    status = 'InProgress'\r\n    AND response_status IS NULL\r\n)\r\nOR\r\n(\r\n    status = 'Completed'\r\n    AND response_status BETWEEN 100 AND 599\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_idempotency_records_status",
                schema: "common",
                table: "idempotency_records",
                sql: "status IN ('InProgress', 'Completed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_idempotency_org_scope_key",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropIndex(
                name: "ux_idempotency_platform_scope_key",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropCheckConstraint(
                name: "ck_idempotency_records_fingerprint_length",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropCheckConstraint(
                name: "ck_idempotency_records_operation_scope",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropCheckConstraint(
                name: "ck_idempotency_records_reservation_token",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropCheckConstraint(
                name: "ck_idempotency_records_response",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropCheckConstraint(
                name: "ck_idempotency_records_status",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "operation_scope",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "reservation_token",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "response_content_type",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "response_headers",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "common",
                table: "idempotency_records");

            migrationBuilder.RenameIndex(
                name: "ix_idempotency_expires_at",
                schema: "common",
                table: "idempotency_records",
                newName: "IX_idempotency_records_expires_at");

            migrationBuilder.AlterColumn<int>(
                name: "response_status",
                schema: "common",
                table: "idempotency_records",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "fingerprint",
                schema: "common",
                table: "idempotency_records",
                type: "citext",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateIndex(
                name: "ux_idempotency_org_key",
                schema: "common",
                table: "idempotency_records",
                columns: new[] { "organization_id", "key" },
                unique: true,
                filter: "organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_idempotency_platform_key",
                schema: "common",
                table: "idempotency_records",
                column: "key",
                unique: true,
                filter: "organization_id IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_idempotency_records_status",
                schema: "common",
                table: "idempotency_records",
                sql: "response_status BETWEEN 100 AND 599");
        }
    }
}
