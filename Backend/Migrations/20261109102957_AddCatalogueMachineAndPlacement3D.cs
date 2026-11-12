using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetFullstack.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogueMachineAndPlacement3D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Machine");

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "MachineConfiguration",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "MinValue",
                table: "MachineConfiguration",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "MaxValue",
                table: "MachineConfiguration",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MachineConfiguration",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "MachineModelId",
                table: "Machine",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "PositionX",
                table: "Machine",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PositionY",
                table: "Machine",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PositionZ",
                table: "Machine",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RotationY",
                table: "Machine",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Scale",
                table: "Machine",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "MachineModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Model3DPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Model3DFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineModelConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinValue = table.Column<double>(type: "double precision", nullable: false),
                    MaxValue = table.Column<double>(type: "double precision", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    MachineModelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineModelConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineModelConfiguration_MachineModel_MachineModelId",
                        column: x => x.MachineModelId,
                        principalTable: "MachineModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Machine_MachineModelId",
                table: "Machine",
                column: "MachineModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineModelConfiguration_MachineModelId",
                table: "MachineModelConfiguration",
                column: "MachineModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machine_MachineModel_MachineModelId",
                table: "Machine",
                column: "MachineModelId",
                principalTable: "MachineModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machine_MachineModel_MachineModelId",
                table: "Machine");

            migrationBuilder.DropTable(
                name: "MachineModelConfiguration");

            migrationBuilder.DropTable(
                name: "MachineModel");

            migrationBuilder.DropIndex(
                name: "IX_Machine_MachineModelId",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "MachineConfiguration");

            migrationBuilder.DropColumn(
                name: "MachineModelId",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "PositionZ",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "RotationY",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "Scale",
                table: "Machine");

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "MachineConfiguration",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "MinValue",
                table: "MachineConfiguration",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "MaxValue",
                table: "MachineConfiguration",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Machine",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
