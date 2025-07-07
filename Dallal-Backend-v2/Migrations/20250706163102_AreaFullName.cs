using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class AreaFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<LocalizedString>(
                name: "FullName",
                table: "Areas",
                type: "jsonb",
                nullable: true
            );

            migrationBuilder.Sql(
                @"
                UPDATE ""Areas""
                SET ""FullName"" = ""Name""
            "
            );

            migrationBuilder.AlterColumn<LocalizedString>(
                name: "FullName",
                table: "Areas",
                nullable: false,
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FullName", table: "Areas");
        }
    }
}
