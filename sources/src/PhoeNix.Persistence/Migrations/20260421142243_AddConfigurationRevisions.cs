using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationRevisions_Configurations_ConfigurationId",
                table: "ConfigurationRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_Inputs_ConfigurationRevisions_ConfigurationRevisionId",
                table: "Inputs");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleValue_ConfigurationRevisions_ConfigurationRevisionId",
                table: "ModuleValue");

            migrationBuilder.DropForeignKey(
                name: "FK_Systems_ConfigurationRevisions_ConfigurationRevisionId",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_Systems_ConfigurationRevisionId",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_ModuleValue_ConfigurationRevisionId",
                table: "ModuleValue");

            migrationBuilder.DropIndex(
                name: "IX_Inputs_ConfigurationRevisionId",
                table: "Inputs");

            migrationBuilder.DropColumn(
                name: "ConfigurationRevisionId",
                table: "Systems");

            migrationBuilder.DropColumn(
                name: "ConfigurationRevisionId",
                table: "ModuleValue");

            migrationBuilder.DropColumn(
                name: "ConfigurationRevisionId",
                table: "Inputs");

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "Configurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ConfigurationRevisions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ConfigurationRevisions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "ConfigurationRevisions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotJson",
                table: "ConfigurationRevisions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationRevisions_Configurations_ConfigurationId",
                table: "ConfigurationRevisions",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationRevisions_Configurations_ConfigurationId",
                table: "ConfigurationRevisions");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "ConfigurationRevisions");

            migrationBuilder.DropColumn(
                name: "SnapshotJson",
                table: "ConfigurationRevisions");

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigurationRevisionId",
                table: "Systems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigurationRevisionId",
                table: "ModuleValue",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigurationRevisionId",
                table: "Inputs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ConfigurationRevisions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ConfigurationRevisions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_Systems_ConfigurationRevisionId",
                table: "Systems",
                column: "ConfigurationRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_ConfigurationRevisionId",
                table: "ModuleValue",
                column: "ConfigurationRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inputs_ConfigurationRevisionId",
                table: "Inputs",
                column: "ConfigurationRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationRevisions_Configurations_ConfigurationId",
                table: "ConfigurationRevisions",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inputs_ConfigurationRevisions_ConfigurationRevisionId",
                table: "Inputs",
                column: "ConfigurationRevisionId",
                principalTable: "ConfigurationRevisions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleValue_ConfigurationRevisions_ConfigurationRevisionId",
                table: "ModuleValue",
                column: "ConfigurationRevisionId",
                principalTable: "ConfigurationRevisions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Systems_ConfigurationRevisions_ConfigurationRevisionId",
                table: "Systems",
                column: "ConfigurationRevisionId",
                principalTable: "ConfigurationRevisions",
                principalColumn: "Id");
        }
    }
}
