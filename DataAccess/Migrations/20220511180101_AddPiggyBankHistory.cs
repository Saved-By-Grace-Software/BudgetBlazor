using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddPiggyBankHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PiggyBanks_Accounts_SavingsAccountId",
                table: "PiggyBanks");

            migrationBuilder.AlterColumn<int>(
                name: "SavingsAccountId",
                table: "PiggyBanks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "PiggyBankHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PiggyBankId = table.Column<int>(type: "int", nullable: false),
                    SavedAmountDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SavedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiggyBankHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiggyBankHistories_PiggyBanks_PiggyBankId",
                        column: x => x.PiggyBankId,
                        principalTable: "PiggyBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PiggyBankHistories_PiggyBankId",
                table: "PiggyBankHistories",
                column: "PiggyBankId");

            migrationBuilder.AddForeignKey(
                name: "FK_PiggyBanks_Accounts_SavingsAccountId",
                table: "PiggyBanks",
                column: "SavingsAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PiggyBanks_Accounts_SavingsAccountId",
                table: "PiggyBanks");

            migrationBuilder.DropTable(
                name: "PiggyBankHistories");

            migrationBuilder.AlterColumn<int>(
                name: "SavingsAccountId",
                table: "PiggyBanks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PiggyBanks_Accounts_SavingsAccountId",
                table: "PiggyBanks",
                column: "SavingsAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
