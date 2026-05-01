using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToModuleTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemUsers");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplates_Name",
                table: "ModuleTemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "SetupSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "ModuleTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Configurations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "AppSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessions_OwnerId",
                table: "SetupSessions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplates_Name_OwnerId",
                table: "ModuleTemplates",
                columns: new[] { "Name", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplates_OwnerId",
                table: "ModuleTemplates",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_OwnerId",
                table: "Machines",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_OwnerId",
                table: "Configurations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_OwnerId",
                table: "AppSettings",
                column: "OwnerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_Users_OwnerId",
                table: "AppSettings",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Configurations_Users_OwnerId",
                table: "Configurations",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Users_OwnerId",
                table: "Machines",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleTemplates_Users_OwnerId",
                table: "ModuleTemplates",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SetupSessions_Users_OwnerId",
                table: "SetupSessions",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_Users_OwnerId",
                table: "AppSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Configurations_Users_OwnerId",
                table: "Configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Users_OwnerId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleTemplates_Users_OwnerId",
                table: "ModuleTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_SetupSessions_Users_OwnerId",
                table: "SetupSessions");

            migrationBuilder.DropIndex(
                name: "IX_SetupSessions_OwnerId",
                table: "SetupSessions");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplates_Name_OwnerId",
                table: "ModuleTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplates_OwnerId",
                table: "ModuleTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Machines_OwnerId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Configurations_OwnerId",
                table: "Configurations");

            migrationBuilder.DropIndex(
                name: "IX_AppSettings_OwnerId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "SetupSessions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ModuleTemplates");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "AppSettings");

            migrationBuilder.CreateTable(
                name: "SystemUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: false),
                    HomePath = table.Column<string>(type: "text", nullable: false),
                    IsNormalUser = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Shell = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplates_Name",
                table: "ModuleTemplates",
                column: "Name",
                unique: true);
        }
    }
}
