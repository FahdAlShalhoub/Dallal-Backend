using System;
using System.Collections.Generic;
using Dallal_Backend_v2.Entities.Submissions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class SubmissionsAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailsDefinitionOption_DetailsDefinition_DetailsDefinition~",
                table: "DetailsDefinitionOption");

            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetail_DetailsDefinitionOption_OptionId",
                table: "ListingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetail_DetailsDefinition_DefinitionId",
                table: "ListingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetail_Listings_ListingId",
                table: "ListingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Brokers_BrokerId",
                table: "Listings");

            migrationBuilder.DropTable(
                name: "Brokers");

            migrationBuilder.DropTable(
                name: "Buyers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListingDetail",
                table: "ListingDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetailsDefinitionOption",
                table: "DetailsDefinitionOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetailsDefinition",
                table: "DetailsDefinition");

            migrationBuilder.RenameTable(
                name: "ListingDetail",
                newName: "ListingDetails");

            migrationBuilder.RenameTable(
                name: "DetailsDefinitionOption",
                newName: "DetailsDefinitionOptions");

            migrationBuilder.RenameTable(
                name: "DetailsDefinition",
                newName: "DetailsDefinitions");

            migrationBuilder.RenameIndex(
                name: "IX_ListingDetail_OptionId",
                table: "ListingDetails",
                newName: "IX_ListingDetails_OptionId");

            migrationBuilder.RenameIndex(
                name: "IX_ListingDetail_ListingId",
                table: "ListingDetails",
                newName: "IX_ListingDetails_ListingId");

            migrationBuilder.RenameIndex(
                name: "IX_ListingDetail_DefinitionId",
                table: "ListingDetails",
                newName: "IX_ListingDetails_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_DetailsDefinitionOption_DetailsDefinitionId",
                table: "DetailsDefinitionOptions",
                newName: "IX_DetailsDefinitionOptions_DetailsDefinitionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListingDetails",
                table: "ListingDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetailsDefinitionOptions",
                table: "DetailsDefinitionOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetailsDefinitions",
                table: "DetailsDefinitions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Changes = table.Column<List<SubmissionChange>>(type: "jsonb", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false),
                    ProfileImage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    AgencyName = table.Column<string>(type: "text", nullable: true),
                    AgencyAddress = table.Column<string>(type: "text", nullable: true),
                    AgencyPhone = table.Column<string>(type: "text", nullable: true),
                    AgencyEmail = table.Column<string>(type: "text", nullable: true),
                    AgencyWebsite = table.Column<string>(type: "text", nullable: true),
                    AgencyLogo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DetailsDefinitionOptions_DetailsDefinitions_DetailsDefiniti~",
                table: "DetailsDefinitionOptions",
                column: "DetailsDefinitionId",
                principalTable: "DetailsDefinitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                table: "ListingDetails",
                column: "OptionId",
                principalTable: "DetailsDefinitionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetails_DetailsDefinitions_DefinitionId",
                table: "ListingDetails",
                column: "DefinitionId",
                principalTable: "DetailsDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetails_Listings_ListingId",
                table: "ListingDetails",
                column: "ListingId",
                principalTable: "Listings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Users_BrokerId",
                table: "Listings",
                column: "BrokerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailsDefinitionOptions_DetailsDefinitions_DetailsDefiniti~",
                table: "DetailsDefinitionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetails_DetailsDefinitionOptions_OptionId",
                table: "ListingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetails_DetailsDefinitions_DefinitionId",
                table: "ListingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ListingDetails_Listings_ListingId",
                table: "ListingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Users_BrokerId",
                table: "Listings");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListingDetails",
                table: "ListingDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetailsDefinitions",
                table: "DetailsDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetailsDefinitionOptions",
                table: "DetailsDefinitionOptions");

            migrationBuilder.RenameTable(
                name: "ListingDetails",
                newName: "ListingDetail");

            migrationBuilder.RenameTable(
                name: "DetailsDefinitions",
                newName: "DetailsDefinition");

            migrationBuilder.RenameTable(
                name: "DetailsDefinitionOptions",
                newName: "DetailsDefinitionOption");

            migrationBuilder.RenameIndex(
                name: "IX_ListingDetails_OptionId",
                table: "ListingDetail",
                newName: "IX_ListingDetail_OptionId");

            migrationBuilder.RenameIndex(
                name: "IX_ListingDetails_ListingId",
                table: "ListingDetail",
                newName: "IX_ListingDetail_ListingId");

            migrationBuilder.RenameIndex(
                name: "IX_ListingDetails_DefinitionId",
                table: "ListingDetail",
                newName: "IX_ListingDetail_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_DetailsDefinitionOptions_DetailsDefinitionId",
                table: "DetailsDefinitionOption",
                newName: "IX_DetailsDefinitionOption_DetailsDefinitionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListingDetail",
                table: "ListingDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetailsDefinition",
                table: "DetailsDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetailsDefinitionOption",
                table: "DetailsDefinitionOption",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Brokers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brokers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    ProfileImage = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DetailsDefinitionOption_DetailsDefinition_DetailsDefinition~",
                table: "DetailsDefinitionOption",
                column: "DetailsDefinitionId",
                principalTable: "DetailsDefinition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetail_DetailsDefinitionOption_OptionId",
                table: "ListingDetail",
                column: "OptionId",
                principalTable: "DetailsDefinitionOption",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetail_DetailsDefinition_DefinitionId",
                table: "ListingDetail",
                column: "DefinitionId",
                principalTable: "DetailsDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListingDetail_Listings_ListingId",
                table: "ListingDetail",
                column: "ListingId",
                principalTable: "Listings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Brokers_BrokerId",
                table: "Listings",
                column: "BrokerId",
                principalTable: "Brokers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
