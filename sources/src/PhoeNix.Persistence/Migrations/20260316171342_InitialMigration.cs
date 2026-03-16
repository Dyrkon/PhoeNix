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
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MacAddress = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    HardwareProfile_SchemaVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftwareSnapshot_SchemaVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    MachineStatus_MachineState = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    MachineStatus_LastContacted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MachineStatus_LastProvisioned = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MachineStatus_LastOrchestrated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MachineStatus_LastConfigured = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetupSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KernelLocation = table.Column<string>(type: "TEXT", nullable: true),
                    InitRdLocation = table.Column<string>(type: "TEXT", nullable: true),
                    CmdLine = table.Column<string>(type: "TEXT", nullable: true),
                    SshPublicKey = table.Column<string>(type: "TEXT", nullable: true),
                    SshCertificatePublicKey = table.Column<string>(type: "TEXT", nullable: true),
                    SshKeyExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SshKeyRevokedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemUsers",
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
                    table.PrimaryKey("PK_SystemUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inputs_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Architecture = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Systems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Systems_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryValueDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InputType = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tests_ModuleTemplates_ModuleTemplateId",
                        column: x => x.ModuleTemplateId,
                        principalTable: "ModuleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetupSessionTargets",
                columns: table => new
                {
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SetupSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CallbackToken = table.Column<string>(type: "TEXT", nullable: true),
                    CallbackTokenExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CallbackTokenRevokedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Stage = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupSessionTargets", x => new { x.SetupSessionId, x.MachineId });
                    table.ForeignKey(
                        name: "FK_SetupSessionTargets_SetupSessions_SetupSessionId",
                        column: x => x.SetupSessionId,
                        principalTable: "SetupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "ModuleValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SystemId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleValue_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleValue_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
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
                    ModuleValueId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                        name: "FK_EntryValues_ModuleValue_ModuleValueId",
                        column: x => x.ModuleValueId,
                        principalTable: "ModuleValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryValueDefinition_ModuleTemplateId",
                table: "EntryValueDefinition",
                column: "ModuleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryValues_ModuleValueId",
                table: "EntryValues",
                column: "ModuleValueId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowInput_InputId",
                table: "FollowInput",
                column: "InputId");

            migrationBuilder.CreateIndex(
                name: "IX_Inputs_ConfigurationId",
                table: "Inputs",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MacAddress",
                table: "Machines",
                column: "MacAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineStatus_MachineState",
                table: "Machines",
                column: "MachineStatus_MachineState");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Title",
                table: "Machines",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_ConfigurationId",
                table: "ModuleValue",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_SystemId",
                table: "ModuleValue",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargets_MachineId",
                table: "SetupSessionTargets",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Systems_ConfigurationId",
                table: "Systems",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_ModuleTemplateId",
                table: "Tests",
                column: "ModuleTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryValueDefinition");

            migrationBuilder.DropTable(
                name: "EntryValues");

            migrationBuilder.DropTable(
                name: "FollowInput");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "SetupSessionTargets");

            migrationBuilder.DropTable(
                name: "SystemUsers");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ModuleValue");

            migrationBuilder.DropTable(
                name: "Inputs");

            migrationBuilder.DropTable(
                name: "SetupSessions");

            migrationBuilder.DropTable(
                name: "ModuleTemplates");

            migrationBuilder.DropTable(
                name: "Systems");

            migrationBuilder.DropTable(
                name: "Configurations");
        }
    }
}
