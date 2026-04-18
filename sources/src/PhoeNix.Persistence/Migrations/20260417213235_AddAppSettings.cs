using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileStorageRootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SshCaKeyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SshCaPrincipal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SshCaCertificateTtlHours = table.Column<double>(type: "double precision", nullable: false),
                    SshCaKeyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeployCaKeyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeployCaKeyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeployCaPrincipal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeployCaDeployUser = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeployCaCertificateTtlDays = table.Column<double>(type: "double precision", nullable: false),
                    HardwareProbeSshExecutable = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HardwareProbeBootstrapUser = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HardwareProbeProbeCommand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HardwareProbeConnectTimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    HardwareProbeProbeTimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    HardwareProbeDisableHostKeyChecking = table.Column<bool>(type: "boolean", nullable: false),
                    InstallerExecutableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstallerTargetUser = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstallerTimeoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    InstallerDisableHostKeyChecking = table.Column<bool>(type: "boolean", nullable: false),
                    InstallerBuildOnTarget = table.Column<bool>(type: "boolean", nullable: false),
                    InstallerCopyHostKeys = table.Column<bool>(type: "boolean", nullable: false),
                    UpdaterBuildHost = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UpdaterUseRemoteSudo = table.Column<bool>(type: "boolean", nullable: false),
                    UpdaterFast = table.Column<bool>(type: "boolean", nullable: false),
                    MonitoringPrometheusEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MonitoringTokenTtlDays = table.Column<double>(type: "double precision", nullable: false),
                    NetbootApiBasePublicUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NetbootHostExecutablePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NetbootListenAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NetbootPort = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
