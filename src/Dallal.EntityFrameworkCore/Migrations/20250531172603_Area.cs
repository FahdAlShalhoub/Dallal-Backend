using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal.Migrations
{
    /// <inheritdoc />
    public partial class Area : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Area_Area_ParentId",
                table: "Area");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Area_AreaId",
                table: "Listings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Area",
                table: "Area");

            migrationBuilder.RenameTable(
                name: "Area",
                newName: "Areas");

            migrationBuilder.RenameIndex(
                name: "IX_Area_ParentId",
                table: "Areas",
                newName: "IX_Areas_ParentId");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Listings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "ListingDetails",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "ListingDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "DetailsDefinitions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "DetailsDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "DetailsDefinitionOptions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "DetailsDefinitionOptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Areas",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Areas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Areas",
                table: "Areas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Areas_ParentId",
                table: "Areas",
                column: "ParentId",
                principalTable: "Areas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Areas_AreaId",
                table: "Listings",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Areas_ParentId",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Areas_AreaId",
                table: "Listings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Areas",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "ListingDetails");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "ListingDetails");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "DetailsDefinitions");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "DetailsDefinitions");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "DetailsDefinitionOptions");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "DetailsDefinitionOptions");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Areas");

            migrationBuilder.RenameTable(
                name: "Areas",
                newName: "Area");

            migrationBuilder.RenameIndex(
                name: "IX_Areas_ParentId",
                table: "Area",
                newName: "IX_Area_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Area",
                table: "Area",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Area_Area_ParentId",
                table: "Area",
                column: "ParentId",
                principalTable: "Area",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Area_AreaId",
                table: "Listings",
                column: "AreaId",
                principalTable: "Area",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
