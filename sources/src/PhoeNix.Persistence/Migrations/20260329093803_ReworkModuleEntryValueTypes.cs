using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoeNix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReworkModuleEntryValueTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeDiscriminator",
                table: "EntryValues");

            migrationBuilder.RenameColumn(
                name: "RangeValue_UpperValue",
                table: "EntryValues",
                newName: "IntegerRangeValue_UpperValue");

            migrationBuilder.RenameColumn(
                name: "RangeValue_Min",
                table: "EntryValues",
                newName: "IntegerRangeValue_Min");

            migrationBuilder.RenameColumn(
                name: "RangeValue_Max",
                table: "EntryValues",
                newName: "IntegerRangeValue_Max");

            migrationBuilder.RenameColumn(
                name: "RangeValue_LowerValue",
                table: "EntryValues",
                newName: "IntegerRangeValue_LowerValue");

            migrationBuilder.AddColumn<bool>(
                name: "AllowLowerValue",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DecimalMax",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DecimalMin",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultLowerValue",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntegerMax",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntegerMin",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueKind",
                table: "ModuleTemplateEntryValueDefinitions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "UpperValue",
                table: "EntryValues",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                table: "EntryValues",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Max",
                table: "EntryValues",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LowerValue",
                table: "EntryValues",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryValueKind",
                table: "EntryValues",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "EntryValues",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowLowerValue",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "DecimalMax",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "DecimalMin",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultLowerValue",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "IntegerMax",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "IntegerMin",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "OptionsJson",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "ValueKind",
                table: "ModuleTemplateEntryValueDefinitions");

            migrationBuilder.DropColumn(
                name: "EntryValueKind",
                table: "EntryValues");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "EntryValues");

            migrationBuilder.RenameColumn(
                name: "IntegerRangeValue_UpperValue",
                table: "EntryValues",
                newName: "RangeValue_UpperValue");

            migrationBuilder.RenameColumn(
                name: "IntegerRangeValue_Min",
                table: "EntryValues",
                newName: "RangeValue_Min");

            migrationBuilder.RenameColumn(
                name: "IntegerRangeValue_Max",
                table: "EntryValues",
                newName: "RangeValue_Max");

            migrationBuilder.RenameColumn(
                name: "IntegerRangeValue_LowerValue",
                table: "EntryValues",
                newName: "RangeValue_LowerValue");

            migrationBuilder.AlterColumn<double>(
                name: "UpperValue",
                table: "EntryValues",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Min",
                table: "EntryValues",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Max",
                table: "EntryValues",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "LowerValue",
                table: "EntryValues",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeDiscriminator",
                table: "EntryValues",
                type: "TEXT",
                maxLength: 21,
                nullable: false,
                defaultValue: "");
        }
    }
}
