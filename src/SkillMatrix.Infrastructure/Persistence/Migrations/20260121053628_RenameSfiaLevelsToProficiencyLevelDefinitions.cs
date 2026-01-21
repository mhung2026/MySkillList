using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSfiaLevelsToProficiencyLevelDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename table instead of drop/create to preserve data
            migrationBuilder.RenameTable(
                name: "SfiaLevels",
                newName: "ProficiencyLevelDefinitions");

            // Drop Vietnamese columns that are no longer needed
            migrationBuilder.DropColumn(name: "LevelNameVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "DescriptionVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "AutonomyVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "InfluenceVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "ComplexityVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "KnowledgeVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "BusinessSkillsVi", table: "ProficiencyLevelDefinitions");
            migrationBuilder.DropColumn(name: "BehavioralIndicatorsVi", table: "ProficiencyLevelDefinitions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename table back
            migrationBuilder.RenameTable(
                name: "ProficiencyLevelDefinitions",
                newName: "SfiaLevels");

            // Add Vietnamese columns back
            migrationBuilder.AddColumn<string>(name: "LevelNameVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "DescriptionVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "AutonomyVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "InfluenceVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ComplexityVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "KnowledgeVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "BusinessSkillsVi", table: "SfiaLevels", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "BehavioralIndicatorsVi", table: "SfiaLevels", type: "text", nullable: true);
        }
    }
}
