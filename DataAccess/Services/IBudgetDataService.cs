using DataAccess.Models;

namespace DataAccess.Services
{
    public interface IBudgetDataService
    {
        // Event to notify subscribers when budget changes occur
        public delegate void NotifyBudgetDataChange();
        public event NotifyBudgetDataChange BudgetDataChanged;

        // Event to notify subscribers when account changes occur
        public delegate void NotifyAccountDataChange();
        public event NotifyAccountDataChange AccountDataChanged;

        // Create
        BudgetMonth Create(int year, int month, Guid user);
        BudgetMonth CreateFromDefault(int year, int month, Guid user);
        List<Account> CreateAccount(Account account, Guid user);

        // Get
        BudgetMonth Get(int budgetMonthId, Guid user);
        BudgetMonth Get(int year, int month, Guid user);
        BudgetMonth GetOrCreate(int year, int month, Guid user);
        List<BudgetMonth> GetAllMonths(Guid user);
        BudgetMonth GetDefaultMonth(Guid user);
        List<Account> GetAllAccounts(Guid user);
        Account GetAccount(int accountId, Guid user);
        List<BudgetItem> GetBudgetItems(int year, int month, Guid user);

        // Update
        BudgetMonth Update(BudgetMonth budgetMonth);
        BudgetCategory Update(BudgetCategory budgetCategory);
        BudgetItem Update(BudgetItem budgetItem);
        void UpdateMonthTotals(BudgetMonth budgetMonth);
        void UpdateMonthTotals(int budgetMonthId);
        Account UpdateAccount(Account account);
        void UpdateAccountHistory(Account account, DateTime balanceDate, decimal balance);

        // Delete
        void Delete(int budgetMonthId);
        void Delete(BudgetItem budgetItem);
        void Delete(BudgetCategory budgetCategory);
        BudgetMonth ResetMonthToDefault(BudgetMonth budgetMonth, Guid user);
        void DeleteAccount(Account account);
        void DeleteTransaction(Transaction transaction);
        void RejectChanges();
    }
}
