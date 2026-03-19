using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrchestrationRelatedChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryValueDefinition");

            migrationBuilder.RenameColumn(
                name: "SelectedInstallDiskByIdPath",
                table: "SetupSessionTargets",
                newName: "SelectedSystemId");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedConfigurationId",
                table: "SetupSessionTargets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ModuleTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "_supportedArchitectures",
                table: "ModuleTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "ModuleTemplateEntryValueDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InputType = table.Column<string>(type: "TEXT", nullable: false),
                    BindingKind = table.Column<string>(type: "TEXT", nullable: false),
                    BindingIndex = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTemplateEntryValueDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleTemplateEntryValueDefinitions_ModuleTemplates_ModuleTemplateId",
                        column: x => x.ModuleTemplateId,
                        principalTable: "ModuleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetupSessionTargetRankedDisks",
                columns: table => new
                {
                    RankIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    SetupSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiskByIdPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupSessionTargetRankedDisks", x => new { x.SetupSessionId, x.MachineId, x.RankIndex });
                    table.ForeignKey(
                        name: "FK_SetupSessionTargetRankedDisks_SetupSessionTargets_SetupSessionId_MachineId",
                        columns: x => new { x.SetupSessionId, x.MachineId },
                        principalTable: "SetupSessionTargets",
                        principalColumns: new[] { "SetupSessionId", "MachineId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargets_SelectedConfigurationId",
                table: "SetupSessionTargets",
                column: "SelectedConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargets_SelectedSystemId",
                table: "SetupSessionTargets",
                column: "SelectedSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_ModuleTemplateId",
                table: "ModuleValue",
                column: "ModuleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId",
                table: "ModuleTemplateEntryValueDefinitions",
                column: "ModuleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargetRankedDisks_DiskByIdPath",
                table: "SetupSessionTargetRankedDisks",
                column: "DiskByIdPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropTable(
                name: "SetupSessionTargetRankedDisks");

            migrationBuilder.DropIndex(
                name: "IX_SetupSessionTargets_SelectedConfigurationId",
                table: "SetupSessionTargets");

            migrationBuilder.DropIndex(
                name: "IX_SetupSessionTargets_SelectedSystemId",
                table: "SetupSessionTargets");

            migrationBuilder.DropIndex(
                name: "IX_ModuleValue_ModuleTemplateId",
                table: "ModuleValue");

            migrationBuilder.DropColumn(
                name: "SelectedConfigurationId",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "_supportedArchitectures",
                table: "ModuleTemplates");

            migrationBuilder.RenameColumn(
                name: "SelectedSystemId",
                table: "SetupSessionTargets",
                newName: "SelectedInstallDiskByIdPath");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ModuleTemplates",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "EntryValueDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InputType = table.Column<int>(type: "INTEGER", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryValueDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryValueDefinition_ModuleTemplates_ModuleTemplateId",
                        column: x => x.ModuleTemplateId,
                        principalTable: "ModuleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryValueDefinition_ModuleTemplateId",
                table: "EntryValueDefinition",
                column: "ModuleTemplateId");
        }
    }
}
