using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddUniqueIndexToAccountHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccountsHistories_BalanceDate_Balance",
                table: "AccountsHistories",
                columns: new[] { "BalanceDate", "Balance" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountsHistories_BalanceDate_Balance",
                table: "AccountsHistories");
        }
    }
}
