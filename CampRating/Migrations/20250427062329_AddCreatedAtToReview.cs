using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampRating.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "Reviews",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reviews",
                newName: "DateCreated");
        }
    }
}
