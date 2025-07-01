using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dallal_Backend_v2
{
    /// <inheritdoc />
    public partial class SubmissionStuff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Changes",
                table: "Submissions",
                newName: "NewData");

            migrationBuilder.AddColumn<string>(
                name: "OldData",
                table: "Submissions",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldData",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "NewData",
                table: "Submissions",
                newName: "Changes");
        }
    }
}
