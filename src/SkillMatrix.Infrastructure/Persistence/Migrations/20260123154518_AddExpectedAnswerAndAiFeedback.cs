using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedAnswerAndAiFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedAnswer",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiFeedback",
                table: "AssessmentResponses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedAnswer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AiFeedback",
                table: "AssessmentResponses");
        }
    }
}
