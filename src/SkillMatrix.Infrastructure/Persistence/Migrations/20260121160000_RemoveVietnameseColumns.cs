using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVietnameseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove Vietnamese columns from SystemEnumValues table
            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "SystemEnumValues");

            migrationBuilder.DropColumn(
                name: "DescriptionVi",
                table: "SystemEnumValues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add Vietnamese columns if rolling back
            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "SystemEnumValues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionVi",
                table: "SystemEnumValues",
                type: "text",
                nullable: true);
        }
    }
}
