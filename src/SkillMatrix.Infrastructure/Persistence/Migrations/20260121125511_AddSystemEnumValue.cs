using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemEnumValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemEnumValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnumType = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NameVi = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DescriptionVi = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SystemEnumValues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemEnumValues_EnumType",
                table: "SystemEnumValues",
                column: "EnumType");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEnumValues_EnumType_Code",
                table: "SystemEnumValues",
                columns: new[] { "EnumType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemEnumValues_EnumType_Value",
                table: "SystemEnumValues",
                columns: new[] { "EnumType", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemEnumValues");
        }
    }
}
