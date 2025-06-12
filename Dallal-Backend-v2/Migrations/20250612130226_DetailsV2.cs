using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class DetailsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                table: "ListingDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "OptionId",
                table: "ListingDetails",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "ListingDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "DetailsDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHiddenInSearch",
                table: "DetailsDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "DetailsDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int[]>(
                name: "PropertyTypes",
                table: "DetailsDefinitions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchBehavior",
                table: "DetailsDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                table: "ListingDetails",
                column: "OptionId",
                principalTable: "DetailsDefinitionOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                table: "ListingDetails");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "ListingDetails");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "DetailsDefinitions");

            migrationBuilder.DropColumn(
                name: "IsHiddenInSearch",
                table: "DetailsDefinitions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "DetailsDefinitions");

            migrationBuilder.DropColumn(
                name: "PropertyTypes",
                table: "DetailsDefinitions");

            migrationBuilder.DropColumn(
                name: "SearchBehavior",
                table: "DetailsDefinitions");

            migrationBuilder.AlterColumn<Guid>(
                name: "OptionId",
                table: "ListingDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                table: "ListingDetails",
                column: "OptionId",
                principalTable: "DetailsDefinitionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
