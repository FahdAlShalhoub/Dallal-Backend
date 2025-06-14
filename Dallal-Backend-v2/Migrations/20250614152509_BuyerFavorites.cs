using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class BuyerFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuyerFavoriteListings",
                columns: table => new
                {
                    FavoriteListingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    FavoritesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerFavoriteListings", x => new { x.FavoriteListingsId, x.FavoritesId });
                    table.ForeignKey(
                        name: "FK_BuyerFavoriteListings_Listings_FavoriteListingsId",
                        column: x => x.FavoriteListingsId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuyerFavoriteListings_Users_FavoritesId",
                        column: x => x.FavoritesId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuyerFavoriteListings_FavoritesId",
                table: "BuyerFavoriteListings",
                column: "FavoritesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyerFavoriteListings");
        }
    }
}
