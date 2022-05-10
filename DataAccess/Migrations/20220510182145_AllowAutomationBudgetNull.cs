using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AllowAutomationBudgetNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Automation_BudgetItem_DefaultBudgetToSetId",
                table: "Automation");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultBudgetToSetId",
                table: "Automation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Automation_BudgetItem_DefaultBudgetToSetId",
                table: "Automation",
                column: "DefaultBudgetToSetId",
                principalTable: "BudgetItem",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Automation_BudgetItem_DefaultBudgetToSetId",
                table: "Automation");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultBudgetToSetId",
                table: "Automation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Automation_BudgetItem_DefaultBudgetToSetId",
                table: "Automation",
                column: "DefaultBudgetToSetId",
                principalTable: "BudgetItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
