using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGitSyncSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Systems_ConfigurationId",
                table: "Systems");

            migrationBuilder.AddColumn<int>(
                name: "GitAuthMethod",
                table: "AppSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GitAuthSecret",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitBranch",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "GitPullDeleteOrphans",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GitPullPollingIntervalMinutes",
                table: "AppSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GitPushNixFiles",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GitPushValidationTier",
                table: "AppSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GitRemoteUrl",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GitSyncMode",
                table: "AppSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Systems_ConfigurationId_Name",
                table: "Systems",
                columns: new[] { "ConfigurationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_Title_OwnerId",
                table: "Configurations",
                columns: new[] { "Title", "OwnerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Systems_ConfigurationId_Name",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_Configurations_Title_OwnerId",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "GitAuthMethod",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitAuthSecret",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitBranch",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitPullDeleteOrphans",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitPullPollingIntervalMinutes",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitPushNixFiles",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitPushValidationTier",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitRemoteUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitSyncMode",
                table: "AppSettings");

            migrationBuilder.CreateIndex(
                name: "IX_Systems_ConfigurationId",
                table: "Systems",
                column: "ConfigurationId");
        }
    }
}
