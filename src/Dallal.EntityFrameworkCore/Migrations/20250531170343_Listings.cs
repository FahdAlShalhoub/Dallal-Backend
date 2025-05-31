using System;
using Dallal.Localization;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal.Migrations
{
    /// <inheritdoc />
    public partial class Listings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Area",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<LocalizedString>(type: "jsonb", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Area_Area_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Area",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetailsDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<LocalizedString>(type: "jsonb", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailsDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_AbpUsers_BrokerId",
                        column: x => x.BrokerId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Listings_Area_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Area",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetailsDefinitionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<LocalizedString>(type: "jsonb", nullable: false),
                    DetailsDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailsDefinitionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailsDefinitionOptions_DetailsDefinitions_DetailsDefiniti~",
                        column: x => x.DetailsDefinitionId,
                        principalTable: "DetailsDefinitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ListingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "DetailsDefinitionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListingDetails_DetailsDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "DetailsDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListingDetails_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Area_ParentId",
                table: "Area",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailsDefinitionOptions_DetailsDefinitionId",
                table: "DetailsDefinitionOptions",
                column: "DetailsDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDetails_DefinitionId",
                table: "ListingDetails",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDetails_ListingId",
                table: "ListingDetails",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDetails_OptionId",
                table: "ListingDetails",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_AreaId",
                table: "Listings",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_BrokerId",
                table: "Listings",
                column: "BrokerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingDetails");

            migrationBuilder.DropTable(
                name: "DetailsDefinitionOptions");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "DetailsDefinitions");

            migrationBuilder.DropTable(
                name: "Area");
        }
    }
}
