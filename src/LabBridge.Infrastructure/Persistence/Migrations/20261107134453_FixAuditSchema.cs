using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameSequence(
                name: "inbound_message_receipt_seq",
                schema: "ingestion",
                newName: "inbound_message_receipt_seq",
                newSchema: "message_ingestion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ingestion");

            migrationBuilder.RenameSequence(
                name: "inbound_message_receipt_seq",
                schema: "message_ingestion",
                newName: "inbound_message_receipt_seq",
                newSchema: "ingestion");
        }
    }
}
