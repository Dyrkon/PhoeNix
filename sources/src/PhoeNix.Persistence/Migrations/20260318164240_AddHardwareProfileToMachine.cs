using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareProfileToMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MachineStatus_MachineState",
                table: "Machines",
                newName: "MachineState");

            migrationBuilder.RenameColumn(
                name: "MachineStatus_LastProvisioned",
                table: "Machines",
                newName: "LastProvisioned");

            migrationBuilder.RenameColumn(
                name: "MachineStatus_LastOrchestrated",
                table: "Machines",
                newName: "LastOrchestrated");

            migrationBuilder.RenameColumn(
                name: "MachineStatus_LastContacted",
                table: "Machines",
                newName: "LastContacted");

            migrationBuilder.RenameColumn(
                name: "MachineStatus_LastConfigured",
                table: "Machines",
                newName: "LastConfigured");

            migrationBuilder.RenameColumn(
                name: "HardwareProfile_SchemaVersion",
                table: "Machines",
                newName: "MemoryTotalBytes");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_MachineStatus_MachineState",
                table: "Machines",
                newName: "IX_Machines_MachineState");

            migrationBuilder.AddColumn<string>(
                name: "SelectedInstallDiskByIdPath",
                table: "SetupSessionTargets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Architecture",
                table: "Machines",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CpuCoreCount",
                table: "Machines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpuModel",
                table: "Machines",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CpuThreadCount",
                table: "Machines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpuVendor",
                table: "Machines",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HardwareObservedAtUtc",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstallDiskSelectionPreference",
                table: "Machines",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MemoryOccupiedSlotCount",
                table: "Machines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemorySlotCount",
                table: "Machines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherboardModel",
                table: "Machines",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherboardVendor",
                table: "Machines",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MachineDisks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StableDevicePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    KernelDevicePath = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BusType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    IsRotational = table.Column<bool>(type: "INTEGER", nullable: true),
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    VramBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsConnected = table.Column<bool>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<Guid>(type: "TEXT", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineDisks");

            migrationBuilder.DropTable(
                name: "MachineGpus");

            migrationBuilder.DropTable(
                name: "MachineMemoryModules");

            migrationBuilder.DropTable(
                name: "MachinePeripherals");

            migrationBuilder.DropColumn(
                name: "SelectedInstallDiskByIdPath",
                table: "SetupSessionTargets");

            migrationBuilder.DropColumn(
                name: "Architecture",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CpuCoreCount",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CpuModel",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CpuThreadCount",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CpuVendor",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "HardwareObservedAtUtc",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "InstallDiskSelectionPreference",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MemoryOccupiedSlotCount",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MemorySlotCount",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MotherboardModel",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MotherboardVendor",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "MachineState",
                table: "Machines",
                newName: "MachineStatus_MachineState");

            migrationBuilder.RenameColumn(
                name: "LastProvisioned",
                table: "Machines",
                newName: "MachineStatus_LastProvisioned");

            migrationBuilder.RenameColumn(
                name: "LastOrchestrated",
                table: "Machines",
                newName: "MachineStatus_LastOrchestrated");

            migrationBuilder.RenameColumn(
                name: "LastContacted",
                table: "Machines",
                newName: "MachineStatus_LastContacted");

            migrationBuilder.RenameColumn(
                name: "LastConfigured",
                table: "Machines",
                newName: "MachineStatus_LastConfigured");

            migrationBuilder.RenameColumn(
                name: "MemoryTotalBytes",
                table: "Machines",
                newName: "HardwareProfile_SchemaVersion");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_MachineState",
                table: "Machines",
                newName: "IX_Machines_MachineStatus_MachineState");
        }
    }
}
