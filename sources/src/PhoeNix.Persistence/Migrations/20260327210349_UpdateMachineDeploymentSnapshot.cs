using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMachineDeploymentSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineDeploymentBoundDisks",
                columns: table => new
                {
                    DiskIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StableDevicePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineDeploymentBoundDisks", x => new { x.MachineId, x.DiskIndex });
                    table.ForeignKey(
                        name: "FK_MachineDeploymentBoundDisks_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineDeploymentBoundDisks_StableDevicePath",
                table: "MachineDeploymentBoundDisks",
                column: "StableDevicePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineDeploymentBoundDisks");
        }
    }
}
