using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelTemplateShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_ModuleTemplates_ModuleTemplateId",
                table: "Tests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tests",
                table: "Tests");

            migrationBuilder.RenameTable(
                name: "Tests",
                newName: "ModuleTemplateTests");

            migrationBuilder.RenameColumn(
                name: "_supportedArchitectures",
                table: "ModuleTemplates",
                newName: "SupportedArchitectures");

            migrationBuilder.RenameColumn(
                name: "_variableNames",
                table: "ModuleTemplateTests",
                newName: "VariableNames");

            migrationBuilder.RenameIndex(
                name: "IX_Tests_ModuleTemplateId",
                table: "ModuleTemplateTests",
                newName: "IX_ModuleTemplateTests_ModuleTemplateId");

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "ModuleTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleTemplateTests",
                table: "ModuleTemplateTests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplates_Name",
                table: "ModuleTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId_Name",
                table: "ModuleTemplateEntryValueDefinitions",
                columns: new[] { "ModuleTemplateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId_Placeholder",
                table: "ModuleTemplateEntryValueDefinitions",
                columns: new[] { "ModuleTemplateId", "Placeholder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateTests_ModuleTemplateId_Name",
                table: "ModuleTemplateTests",
                columns: new[] { "ModuleTemplateId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleTemplateTests_ModuleTemplates_ModuleTemplateId",
                table: "ModuleTemplateTests",
                column: "ModuleTemplateId",
                principalTable: "ModuleTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleTemplateTests_ModuleTemplates_ModuleTemplateId",
                table: "ModuleTemplateTests");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplates_Name",
                table: "ModuleTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId_Name",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId_Placeholder",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleTemplateTests",
                table: "ModuleTemplateTests");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTemplateTests_ModuleTemplateId_Name",
                table: "ModuleTemplateTests");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "ModuleTemplates");

            migrationBuilder.RenameTable(
                name: "ModuleTemplateTests",
                newName: "Tests");

            migrationBuilder.RenameColumn(
                name: "SupportedArchitectures",
                table: "ModuleTemplates",
                newName: "_supportedArchitectures");

            migrationBuilder.RenameColumn(
                name: "VariableNames",
                table: "Tests",
                newName: "_variableNames");

            migrationBuilder.RenameIndex(
                name: "IX_ModuleTemplateTests_ModuleTemplateId",
                table: "Tests",
                newName: "IX_Tests_ModuleTemplateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tests",
                table: "Tests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_ModuleTemplates_ModuleTemplateId",
                table: "Tests",
                column: "ModuleTemplateId",
                principalTable: "ModuleTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
