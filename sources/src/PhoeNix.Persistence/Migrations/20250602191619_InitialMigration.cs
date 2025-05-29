using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
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
                name: "Inputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FollowsId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inputs_Inputs_FollowsId",
                        column: x => x.FollowsId,
                        principalTable: "Inputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Architecture = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Systems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsNormalUser = table.Column<bool>(type: "INTEGER", nullable: false),
                    HomePath = table.Column<string>(type: "TEXT", nullable: false),
                    Group = table.Column<string>(type: "TEXT", nullable: false),
                    Uid = table.Column<uint>(type: "INTEGER", nullable: false),
                    Shell = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
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
                name: "ConfigurationModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationModules_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Placeholder = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TypeDiscriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    UpperValue = table.Column<double>(type: "REAL", nullable: true),
                    LowerValue = table.Column<double>(type: "REAL", nullable: true),
                    Max = table.Column<double>(type: "REAL", nullable: true),
                    Min = table.Column<double>(type: "REAL", nullable: true),
                    RangeValue_UpperValue = table.Column<int>(type: "INTEGER", nullable: true),
                    RangeValue_LowerValue = table.Column<int>(type: "INTEGER", nullable: true),
                    RangeValue_Max = table.Column<int>(type: "INTEGER", nullable: true),
                    RangeValue_Min = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryValues_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
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
                name: "ConfigurationSystems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationSystems_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationSystems_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemModules_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
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
                name: "IX_ConfigurationModules_ConfigurationId",
                table: "ConfigurationModules",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationModules_ModuleId",
                table: "ConfigurationModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSystems_ConfigurationId",
                table: "ConfigurationSystems",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSystems_SystemId",
                table: "ConfigurationSystems",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryValues_ModuleId",
                table: "EntryValues",
                column: "ModuleId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Inputs_FollowsId",
                table: "Inputs",
                column: "FollowsId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemModules_ModuleId",
                table: "SystemModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemModules_SystemId",
                table: "SystemModules",
                column: "SystemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationHomes");

            migrationBuilder.DropTable(
                name: "ConfigurationInput");

            migrationBuilder.DropTable(
                name: "ConfigurationModules");

            migrationBuilder.DropTable(
                name: "ConfigurationSystems");

            migrationBuilder.DropTable(
                name: "EntryValues");

            migrationBuilder.DropTable(
                name: "HomeModules");

            migrationBuilder.DropTable(
                name: "HomeUsers");

            migrationBuilder.DropTable(
                name: "SystemModules");

            migrationBuilder.DropTable(
                name: "Inputs");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Homes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Systems");
        }
    }
}
