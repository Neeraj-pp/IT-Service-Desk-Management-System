using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RaisedBy",
                table: "ServiceRequests",
                column: "RaisedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RaisedOn",
                table: "ServiceRequests",
                column: "RaisedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ReqStatus",
                table: "ServiceRequests",
                column: "ReqStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PerformedAt",
                table: "AuditLogs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RequestId",
                table: "AuditLogs",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RaisedBy",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RaisedOn",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ReqStatus",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_PerformedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_RequestId",
                table: "AuditLogs");
        }
    }
}
