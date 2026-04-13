using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    MacAddress = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Architecture = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InstallDiskSelectionPreference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HardwareObservedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CpuVendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CpuModel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CpuCoreCount = table.Column<int>(type: "integer", nullable: true),
                    CpuThreadCount = table.Column<int>(type: "integer", nullable: true),
                    MotherboardVendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MotherboardModel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MemoryTotalBytes = table.Column<long>(type: "bigint", nullable: true),
                    MemorySlotCount = table.Column<int>(type: "integer", nullable: true),
                    MemoryOccupiedSlotCount = table.Column<int>(type: "integer", nullable: true),
                    SoftwareSnapshot_SchemaVersion = table.Column<int>(type: "integer", nullable: true),
                    ProvisionedConfigurationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfigurationTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProvisionedSystemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SystemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProvisionedIpAddress = table.Column<string>(type: "text", nullable: true),
                    ProvisionedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MachineState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastContacted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastProvisioned = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastOrchestrated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastConfigured = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SupportedArchitectures = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetupSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KernelLocation = table.Column<string>(type: "text", nullable: true),
                    InitRdLocation = table.Column<string>(type: "text", nullable: true),
                    CmdLine = table.Column<string>(type: "text", nullable: true),
                    SshPublicKey = table.Column<string>(type: "text", nullable: true),
                    SshCertificatePublicKey = table.Column<string>(type: "text", nullable: true),
                    SshKeyExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SshKeyRevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsNormalUser = table.Column<bool>(type: "boolean", nullable: false),
                    HomePath = table.Column<string>(type: "text", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: false),
                    Uid = table.Column<long>(type: "bigint", nullable: false),
                    Shell = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Architecture = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                name: "MachineDeploymentBoundDisks",
                columns: table => new
                {
                    DiskIndex = table.Column<int>(type: "integer", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    StableDevicePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineDeploymentBoundDisks", x => new { x.MachineId, x.DiskIndex });
                    table.ForeignKey(
                        name: "FK_MachineDeploymentBoundDisks_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachineDisks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StableDevicePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KernelDevicePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Vendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BusType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsRotational = table.Column<bool>(type: "boolean", nullable: true),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineDisks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineDisks_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachineGpus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Vendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    VramBytes = table.Column<long>(type: "bigint", nullable: true),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineGpus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineGpus_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachineMemoryModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineMemoryModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineMemoryModules_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachinePeripherals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachinePeripherals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachinePeripherals_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleTemplateEntryValueDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BindingKind = table.Column<string>(type: "text", nullable: false),
                    ValueKind = table.Column<string>(type: "text", nullable: false),
                    DefaultValue = table.Column<string>(type: "text", nullable: true),
                    DefaultLowerValue = table.Column<string>(type: "text", nullable: true),
                    IntegerMin = table.Column<int>(type: "integer", nullable: true),
                    IntegerMax = table.Column<int>(type: "integer", nullable: true),
                    DecimalMin = table.Column<decimal>(type: "numeric", nullable: true),
                    DecimalMax = table.Column<decimal>(type: "numeric", nullable: true),
                    AllowLowerValue = table.Column<bool>(type: "boolean", nullable: false),
                    OptionsJson = table.Column<string>(type: "text", nullable: true),
                    BindingIndex = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTemplateEntryValueDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleTemplateEntryValueDefinitions_ModuleTemplates_ModuleT~",
                        column: x => x.ModuleTemplateId,
                        principalTable: "ModuleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleTemplateTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariableNames = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTemplateTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleTemplateTests_ModuleTemplates_ModuleTemplateId",
                        column: x => x.ModuleTemplateId,
                        principalTable: "ModuleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetupSessionTargets",
                columns: table => new
                {
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetupSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallbackToken = table.Column<string>(type: "text", nullable: true),
                    CallbackTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CallbackTokenRevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    LastTransitionAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastErrorDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LastErrorSource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastErrorAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    SelectedSystemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedConfigurationId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InputId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FollowValue = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SystemId = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "SetupSessionTargetRankedDisks",
                columns: table => new
                {
                    RankIndex = table.Column<int>(type: "integer", nullable: false),
                    SetupSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiskByIdPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupSessionTargetRankedDisks", x => new { x.SetupSessionId, x.MachineId, x.RankIndex });
                    table.ForeignKey(
                        name: "FK_SetupSessionTargetRankedDisks_SetupSessionTargets_SetupSess~",
                        columns: x => new { x.SetupSessionId, x.MachineId },
                        principalTable: "SetupSessionTargets",
                        principalColumns: new[] { "SetupSessionId", "MachineId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModuleValueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    EntryValueKind = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    UpperValue = table.Column<decimal>(type: "numeric", nullable: true),
                    LowerValue = table.Column<decimal>(type: "numeric", nullable: true),
                    Min = table.Column<decimal>(type: "numeric", nullable: true),
                    Max = table.Column<decimal>(type: "numeric", nullable: true),
                    IntegerRangeValue_UpperValue = table.Column<int>(type: "integer", nullable: true),
                    IntegerRangeValue_LowerValue = table.Column<int>(type: "integer", nullable: true),
                    IntegerRangeValue_Min = table.Column<int>(type: "integer", nullable: true),
                    IntegerRangeValue_Max = table.Column<int>(type: "integer", nullable: true),
                    Options = table.Column<string[]>(type: "text[]", nullable: true)
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
                name: "IX_MachineDeploymentBoundDisks_StableDevicePath",
                table: "MachineDeploymentBoundDisks",
                column: "StableDevicePath");

            migrationBuilder.CreateIndex(
                name: "IX_MachineDisks_MachineId",
                table: "MachineDisks",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineGpus_MachineId",
                table: "MachineGpus",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineMemoryModules_MachineId",
                table: "MachineMemoryModules",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachinePeripherals_MachineId",
                table: "MachinePeripherals",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MacAddress",
                table: "Machines",
                column: "MacAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineState",
                table: "Machines",
                column: "MachineState");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Title",
                table: "Machines",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId",
                table: "ModuleTemplateEntryValueDefinitions",
                column: "ModuleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId_Name",
                table: "ModuleTemplateEntryValueDefinitions",
                columns: new[] { "ModuleTemplateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateEntryValueDefinitions_ModuleTemplateId_Placeh~",
                table: "ModuleTemplateEntryValueDefinitions",
                columns: new[] { "ModuleTemplateId", "Placeholder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplates_Name",
                table: "ModuleTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateTests_ModuleTemplateId",
                table: "ModuleTemplateTests",
                column: "ModuleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTemplateTests_ModuleTemplateId_Name",
                table: "ModuleTemplateTests",
                columns: new[] { "ModuleTemplateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_ConfigurationId",
                table: "ModuleValue",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_ModuleTemplateId",
                table: "ModuleValue",
                column: "ModuleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleValue_SystemId",
                table: "ModuleValue",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptOnUtc_OccurredOnUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "NextAttemptOnUtc", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargetRankedDisks_DiskByIdPath",
                table: "SetupSessionTargetRankedDisks",
                column: "DiskByIdPath");

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargets_MachineId",
                table: "SetupSessionTargets",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargets_SelectedConfigurationId",
                table: "SetupSessionTargets",
                column: "SelectedConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupSessionTargets_SelectedSystemId",
                table: "SetupSessionTargets",
                column: "SelectedSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Systems_ConfigurationId",
                table: "Systems",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedName",
                table: "Users",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryValues");

            migrationBuilder.DropTable(
                name: "FollowInput");

            migrationBuilder.DropTable(
                name: "MachineDeploymentBoundDisks");

            migrationBuilder.DropTable(
                name: "MachineDisks");

            migrationBuilder.DropTable(
                name: "MachineGpus");

            migrationBuilder.DropTable(
                name: "MachineMemoryModules");

            migrationBuilder.DropTable(
                name: "MachinePeripherals");

            migrationBuilder.DropTable(
                name: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropTable(
                name: "ModuleTemplateTests");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "SetupSessionTargetRankedDisks");

            migrationBuilder.DropTable(
                name: "SystemUsers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ModuleValue");

            migrationBuilder.DropTable(
                name: "Inputs");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "ModuleTemplates");

            migrationBuilder.DropTable(
                name: "SetupSessionTargets");

            migrationBuilder.DropTable(
                name: "Systems");

            migrationBuilder.DropTable(
                name: "SetupSessions");

            migrationBuilder.DropTable(
                name: "Configurations");
        }
    }
}
