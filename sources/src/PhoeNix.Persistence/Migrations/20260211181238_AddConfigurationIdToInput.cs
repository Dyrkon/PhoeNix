using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationIdToInput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inputs_Inputs_FollowsId",
                table: "Inputs");

            migrationBuilder.DropTable(
                name: "ConfigurationHomes");

            migrationBuilder.DropTable(
                name: "ConfigurationInput");

            migrationBuilder.DropTable(
                name: "HomeModules");

            migrationBuilder.DropTable(
                name: "HomeUsers");

            migrationBuilder.DropTable(
                name: "Homes");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTests_ModuleId",
                table: "ModuleTests");

            migrationBuilder.DropIndex(
                name: "IX_Inputs_FollowsId",
                table: "Inputs");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "ModuleTests");

            migrationBuilder.DropColumn(
                name: "FollowsId",
                table: "Inputs");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ModuleTests",
                newName: "TestId");

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigurationId",
                table: "Inputs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FollowInput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InputId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FollowName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FollowValue = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowInput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowInput_Inputs_InputId",
                        column: x => x.InputId,
                        principalTable: "Inputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTests_ModuleId",
                table: "ModuleTests",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTests_TestId",
                table: "ModuleTests",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_Inputs_ConfigurationId",
                table: "Inputs",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowInput_InputId",
                table: "FollowInput",
                column: "InputId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inputs_Configurations_ConfigurationId",
                table: "Inputs",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleTests_Tests_TestId",
                table: "ModuleTests",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inputs_Configurations_ConfigurationId",
                table: "Inputs");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleTests_Tests_TestId",
                table: "ModuleTests");

            migrationBuilder.DropTable(
                name: "FollowInput");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTests_ModuleId",
                table: "ModuleTests");

            migrationBuilder.DropIndex(
                name: "IX_ModuleTests_TestId",
                table: "ModuleTests");

            migrationBuilder.DropIndex(
                name: "IX_Inputs_ConfigurationId",
                table: "Inputs");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "Inputs");

            migrationBuilder.RenameColumn(
                name: "TestId",
                table: "ModuleTests",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ModuleTests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FollowsId",
                table: "Inputs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfigurationInput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InputId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationInput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationInput_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationInput_Inputs_InputId",
                        column: x => x.InputId,
                        principalTable: "Inputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Homes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationHomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HomeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationHomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationHomes_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationHomes_Homes_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Homes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HomeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeModules_Homes_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Homes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HomeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HomeId1 = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeUsers_Homes_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Homes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeUsers_Homes_HomeId1",
                        column: x => x.HomeId1,
                        principalTable: "Homes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HomeUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTests_ModuleId",
                table: "ModuleTests",
                column: "ModuleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inputs_FollowsId",
                table: "Inputs",
                column: "FollowsId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationHomes_ConfigurationId",
                table: "ConfigurationHomes",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationHomes_HomeId",
                table: "ConfigurationHomes",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationInput_ConfigurationId",
                table: "ConfigurationInput",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationInput_InputId",
                table: "ConfigurationInput",
                column: "InputId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeModules_HomeId",
                table: "HomeModules",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeModules_ModuleId",
                table: "HomeModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeUsers_HomeId",
                table: "HomeUsers",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeUsers_HomeId1",
                table: "HomeUsers",
                column: "HomeId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeUsers_UserId",
                table: "HomeUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inputs_Inputs_FollowsId",
                table: "Inputs",
                column: "FollowsId",
                principalTable: "Inputs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
