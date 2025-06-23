using System.Collections.Generic;
using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class DocumentsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing columns
            migrationBuilder.DropColumn(name: "ProfileImage", table: "Users");

            migrationBuilder.DropColumn(name: "Videos", table: "Listings");

            migrationBuilder.DropColumn(name: "Images", table: "Listings");

            // Add columns with new types
            migrationBuilder.AddColumn<Document>(
                name: "ProfileImage",
                table: "Users",
                type: "jsonb",
                nullable: true
            );

            migrationBuilder.AddColumn<List<Document>>(
                name: "Videos",
                table: "Listings",
                type: "jsonb",
                defaultValue: new List<Document>(),
                nullable: false
            );

            migrationBuilder.AddColumn<List<Document>>(
                name: "Images",
                table: "Listings",
                type: "jsonb",
                defaultValue: new List<Document>(),
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new columns
            migrationBuilder.DropColumn(name: "ProfileImage", table: "Users");

            migrationBuilder.DropColumn(name: "Videos", table: "Listings");

            migrationBuilder.DropColumn(name: "Images", table: "Listings");

            // Add columns with old types
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Users",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<List<string>>(
                name: "Videos",
                table: "Listings",
                type: "text[]",
                nullable: false
            );

            migrationBuilder.AddColumn<List<string>>(
                name: "Images",
                table: "Listings",
                type: "text[]",
                nullable: false
            );
        }
    }
}
