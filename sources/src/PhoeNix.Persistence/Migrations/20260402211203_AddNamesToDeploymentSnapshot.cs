using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNamesToDeploymentSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigurationTitle",
                table: "Machines",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemName",
                table: "Machines",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigurationTitle",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SystemName",
                table: "Machines");
        }
    }
}
