using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSfiaLevelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SfiaLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelName = table.Column<string>(type: "text", nullable: false),
                    LevelNameVi = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DescriptionVi = table.Column<string>(type: "text", nullable: true),
                    Autonomy = table.Column<string>(type: "text", nullable: true),
                    AutonomyVi = table.Column<string>(type: "text", nullable: true),
                    Influence = table.Column<string>(type: "text", nullable: true),
                    InfluenceVi = table.Column<string>(type: "text", nullable: true),
                    Complexity = table.Column<string>(type: "text", nullable: true),
                    ComplexityVi = table.Column<string>(type: "text", nullable: true),
                    Knowledge = table.Column<string>(type: "text", nullable: true),
                    KnowledgeVi = table.Column<string>(type: "text", nullable: true),
                    BusinessSkills = table.Column<string>(type: "text", nullable: true),
                    BusinessSkillsVi = table.Column<string>(type: "text", nullable: true),
                    BehavioralIndicators = table.Column<string>(type: "text", nullable: true),
                    BehavioralIndicatorsVi = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfiaLevels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SfiaLevels");
        }
    }
}
