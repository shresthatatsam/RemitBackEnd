using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EverestRemit.Api.Migrations
{
    /// <inheritdoc />
    public partial class kyc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "KycApplications",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "KycApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "KycApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "KycApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "KycApplications");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "KycApplications");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "KycApplications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "KycApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
