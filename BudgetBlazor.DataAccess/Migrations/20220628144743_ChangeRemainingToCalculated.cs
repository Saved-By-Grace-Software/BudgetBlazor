using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ChangeRemainingToCalculated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remaining",
                table: "BudgetItem");

            migrationBuilder.DropColumn(
                name: "Remaining",
                table: "BudgetCategory");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Remaining",
                table: "BudgetItem",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Remaining",
                table: "BudgetCategory",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
