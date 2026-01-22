using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGradingRubricToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GradingRubric",
                table: "Questions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GradingRubric",
                table: "Questions",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
