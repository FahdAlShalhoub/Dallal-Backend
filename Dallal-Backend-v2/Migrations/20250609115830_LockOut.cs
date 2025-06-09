using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class LockOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutUntil",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoginAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockoutUntil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginAttempts",
                table: "Users");
        }
    }
}
