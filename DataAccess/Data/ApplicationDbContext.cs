using DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BudgetMonth> BudgetMonths { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountHistory> AccountsHistories { get; set; }
        public DbSet<AutomationCategory> AutomationCategories { get; set; }
        public DbSet<PiggyBank> PiggyBanks { get; set; }
        public DbSet<PiggyBankHistory> PiggyBankHistories { get; set; }
    }
}