using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisioningDetailsToMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastErrorAtUtc",
                table: "SetupSessionTargets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorCode",
                table: "SetupSessionTargets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorDescription",
                table: "SetupSessionTargets",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorSource",
                table: "SetupSessionTargets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTransitionAtUtc",
                table: "SetupSessionTargets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisionedAtUtc",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvisionedConfigurationId",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisionedIpAddress",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvisionedSystemId",
                table: "Machines",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastErrorAtUtc",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "LastErrorCode",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "LastErrorDescription",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "LastErrorSource",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "LastTransitionAtUtc",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "ProvisionedAtUtc",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ProvisionedConfigurationId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ProvisionedIpAddress",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ProvisionedSystemId",
                table: "Machines");
        }
    }
}
