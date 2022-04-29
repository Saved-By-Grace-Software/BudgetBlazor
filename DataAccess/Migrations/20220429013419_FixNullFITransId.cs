using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class FixNullFITransId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Name_Amount_TransactionDate_User_FITransactionId",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "FITransactionId",
                table: "Transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Name_Amount_TransactionDate_User_FITransactionId",
                table: "Transactions",
                columns: new[] { "Name", "Amount", "TransactionDate", "User", "FITransactionId" },
                unique: true,
                filter: "[FITransactionId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Name_Amount_TransactionDate_User_FITransactionId",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "FITransactionId",
                table: "Transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Name_Amount_TransactionDate_User_FITransactionId",
                table: "Transactions",
                columns: new[] { "Name", "Amount", "TransactionDate", "User", "FITransactionId" },
                unique: true);
        }
    }
}
