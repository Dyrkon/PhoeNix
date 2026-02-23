using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisioningSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackToken",
                table: "ProvisioningSessions");

            migrationBuilder.DropColumn(
                name: "CallbackTokenExpiresAtUtc",
                table: "ProvisioningSessions");

            migrationBuilder.DropColumn(
                name: "CallbackTokenIsRevokedUtc",
                table: "ProvisioningSessions");

            migrationBuilder.DropColumn(
                name: "ProvisioningStage",
                table: "ProvisioningSessions");

            migrationBuilder.RenameColumn(
                name: "SshKeyIsRevokedAt",
                table: "ProvisioningSessions",
                newName: "SshKeyRevokedAtUtc");

            migrationBuilder.RenameColumn(
                name: "SshKeyExpiresAt",
                table: "ProvisioningSessions",
                newName: "SshKeyExpiresAtUtc");

            migrationBuilder.CreateTable(
                name: "ProvisioningSessionTargets",
                columns: table => new
                {
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProvisioningSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CallbackToken = table.Column<string>(type: "TEXT", nullable: false),
                    CallbackTokenExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CallbackTokenRevokedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Stage = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisioningSessionTargets", x => new { x.ProvisioningSessionId, x.MachineId });
                    table.ForeignKey(
                        name: "FK_ProvisioningSessionTargets_ProvisioningSessions_ProvisioningSessionId",
                        column: x => x.ProvisioningSessionId,
                        principalTable: "ProvisioningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningSessionTargets_MachineId",
                table: "ProvisioningSessionTargets",
                column: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvisioningSessionTargets");

            migrationBuilder.RenameColumn(
                name: "SshKeyRevokedAtUtc",
                table: "ProvisioningSessions",
                newName: "SshKeyIsRevokedAt");

            migrationBuilder.RenameColumn(
                name: "SshKeyExpiresAtUtc",
                table: "ProvisioningSessions",
                newName: "SshKeyExpiresAt");

            migrationBuilder.AddColumn<string>(
                name: "CallbackToken",
                table: "ProvisioningSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallbackTokenExpiresAtUtc",
                table: "ProvisioningSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallbackTokenIsRevokedUtc",
                table: "ProvisioningSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisioningStage",
                table: "ProvisioningSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
