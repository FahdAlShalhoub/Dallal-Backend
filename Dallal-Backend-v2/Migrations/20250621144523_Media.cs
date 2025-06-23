using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class Media : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Images",
                table: "Listings",
                type: "text[]",
                defaultValue: new List<string>(),
                nullable: false
            );

            migrationBuilder.AddColumn<List<string>>(
                name: "Videos",
                table: "Listings",
                type: "text[]",
                defaultValue: new List<string>(),
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Images", table: "Listings");

            migrationBuilder.DropColumn(name: "Videos", table: "Listings");
        }
    }
}
