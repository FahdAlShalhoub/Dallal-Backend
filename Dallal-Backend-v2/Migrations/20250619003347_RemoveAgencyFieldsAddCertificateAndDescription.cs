using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class RemoveAgencyFieldsAddCertificateAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyAddress",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "AgencyEmail",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "AgencyLogo",
                table: "Brokers");

            migrationBuilder.RenameColumn(
                name: "AgencyWebsite",
                table: "Brokers",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "AgencyPhone",
                table: "Brokers",
                newName: "CertificateNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Brokers",
                newName: "AgencyWebsite");

            migrationBuilder.RenameColumn(
                name: "CertificateNumber",
                table: "Brokers",
                newName: "AgencyPhone");

            migrationBuilder.AddColumn<string>(
                name: "AgencyAddress",
                table: "Brokers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyEmail",
                table: "Brokers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyLogo",
                table: "Brokers",
                type: "text",
                nullable: true);
        }
    }
}
