using System;
using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class ListingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetailsDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<LocalizedString>(type: "jsonb", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailsDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetailsDefinitionOption",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<LocalizedString>(type: "jsonb", nullable: false),
                    DetailsDefinitionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailsDefinitionOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailsDefinitionOption_DetailsDefinition_DetailsDefinition~",
                        column: x => x.DetailsDefinitionId,
                        principalTable: "DetailsDefinition",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ListingDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingDetail_DetailsDefinitionOption_OptionId",
                        column: x => x.OptionId,
                        principalTable: "DetailsDefinitionOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListingDetail_DetailsDefinition_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "DetailsDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListingDetail_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailsDefinitionOption_DetailsDefinitionId",
                table: "DetailsDefinitionOption",
                column: "DetailsDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDetail_DefinitionId",
                table: "ListingDetail",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDetail_ListingId",
                table: "ListingDetail",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDetail_OptionId",
                table: "ListingDetail",
                column: "OptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingDetail");

            migrationBuilder.DropTable(
                name: "DetailsDefinitionOption");

            migrationBuilder.DropTable(
                name: "DetailsDefinition");
        }
    }
}
