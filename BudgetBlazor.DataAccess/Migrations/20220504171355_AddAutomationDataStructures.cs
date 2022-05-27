using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddAutomationDataStructures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    User = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Automation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsStrict = table.Column<bool>(type: "bit", nullable: false),
                    DefaultBudgetToSetId = table.Column<int>(type: "int", nullable: false),
                    User = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutomationCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation_AutomationCategories_AutomationCategoryId",
                        column: x => x.AutomationCategoryId,
                        principalTable: "AutomationCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Automation_BudgetItem_DefaultBudgetToSetId",
                        column: x => x.DefaultBudgetToSetId,
                        principalTable: "BudgetItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainsText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StopAfterTrigger = table.Column<bool>(type: "bit", nullable: false),
                    AutomationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRule_Automation_AutomationId",
                        column: x => x.AutomationId,
                        principalTable: "Automation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Automation_AutomationCategoryId",
                table: "Automation",
                column: "AutomationCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation_DefaultBudgetToSetId",
                table: "Automation",
                column: "DefaultBudgetToSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRule_AutomationId",
                table: "AutomationRule",
                column: "AutomationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRule");

            migrationBuilder.DropTable(
                name: "Automation");

            migrationBuilder.DropTable(
                name: "AutomationCategories");
        }
    }
}
