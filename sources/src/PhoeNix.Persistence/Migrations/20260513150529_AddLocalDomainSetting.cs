using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalDomainSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalDomain",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalDomain",
                table: "AppSettings");
        }
    }
}
