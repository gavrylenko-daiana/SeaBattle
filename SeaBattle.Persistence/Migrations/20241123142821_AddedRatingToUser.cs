using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaBattle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedRatingToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Users");
        }
    }
}
