using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleTemplateRequiredInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleTemplateRequiredInputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTemplateRequiredInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleTemplateRequiredInputs_ModuleTemplates_ModuleTemplate~",
                        column: x => x.ModuleTemplateId,
                        principalTable: "ModuleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateRequiredInputs_ModuleTemplateId",
                table: "ModuleTemplateRequiredInputs",
                column: "ModuleTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleTemplateRequiredInputs");
        }
    }
}
