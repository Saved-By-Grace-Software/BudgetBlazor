using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ChangeTransactionLinkage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BudgetItem_BudgetItemId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "BudgetItemId",
                table: "Transactions",
                newName: "BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BudgetItemId",
                table: "Transactions",
                newName: "IX_Transactions_BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BudgetItem_BudgetId",
                table: "Transactions",
                column: "BudgetId",
                principalTable: "BudgetItem",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BudgetItem_BudgetId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "BudgetId",
                table: "Transactions",
                newName: "BudgetItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transactions",
                newName: "IX_Transactions_BudgetItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BudgetItem_BudgetItemId",
                table: "Transactions",
                column: "BudgetItemId",
                principalTable: "BudgetItem",
                principalColumn: "Id");
        }
    }
}
