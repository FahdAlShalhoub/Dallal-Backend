using System.Collections.Generic;
using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class Documents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayCategory",
                table: "DetailsDefinitions",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<List<Document>>(
                name: "Documents",
                table: "Brokers",
                type: "jsonb",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DisplayCategory", table: "DetailsDefinitions");

            migrationBuilder.DropColumn(name: "Documents", table: "Brokers");
        }
    }
}
