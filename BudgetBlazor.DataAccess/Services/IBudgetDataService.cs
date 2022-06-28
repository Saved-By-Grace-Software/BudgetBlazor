using BudgetBlazor.DataAccess.Models;

namespace BudgetBlazor.DataAccess.Services
{
    public interface IBudgetDataService
    {
        // Event to notify subscribers when budget changes occur
        public delegate void NotifyBudgetDataChange();
        public event NotifyBudgetDataChange BudgetDataChanged;

        // Event to notify subscribers when account changes occur
        public delegate void NotifyAccountDataChange();
        public event NotifyAccountDataChange AccountDataChanged;

        // Event to notify subscribers when automation changes occur
        public delegate void NotifyAutomationDataChange();
        public event NotifyAutomationDataChange AutomationDataChanged;

        // Create
        BudgetMonth Create(int year, int month, Guid user);
        BudgetMonth CreateFromDefault(int year, int month, Guid user);
        List<Account> CreateAccount(Account account, Guid user);
        AutomationCategory Create(AutomationCategory category);
        PiggyBank Create(PiggyBank piggyBank);

        // Get
        BudgetMonth Get(int budgetMonthId, Guid user);
        BudgetMonth Get(int year, int month, Guid user);
        BudgetMonth GetOrCreate(int year, int month, Guid user);
        List<BudgetMonth> GetAllMonths(Guid user);
        BudgetMonth GetDefaultMonth(Guid user);
        List<Account> GetAllAccounts(Guid user);
        List<Account> GetAllPiggyBankAccounts(Guid user);
        List<PiggyBank> GetAllPiggyBanks(Guid user);
        Account GetAccount(int accountId, Guid user);
        List<BudgetItem> GetBudgetItems(int year, int month, Guid user);
        List<BudgetCategory> GetBudgetCategories(int year, int month, Guid user);
        List<AutomationCategory> GetAutomationCategories(Guid user);
        List<Automation> GetAutomations(Guid user);
        List<Transaction> GetTransactions(Guid user);
        BudgetItem GetMatchingBudgetItem(BudgetItem defaultMonthBudgetItem, int month, int year, Guid user);
        List<AccountHistory> GetAccountHistory(int accountId);

        // Update
        BudgetMonth Update(BudgetMonth budgetMonth);
        BudgetCategory Update(BudgetCategory budgetCategory);
        BudgetItem Update(BudgetItem budgetItem);
        AutomationCategory Update(AutomationCategory category);
        Account Update(Account account);
        Transaction Update(Transaction transaction);
        PiggyBank Update(PiggyBank piggyBank);
        Account AddTransactionToAccount(Account account, Transaction transaction);
        void UpdateMonthTotals(int year, int month, Guid user);
        void UpdateMonthTotals(BudgetMonth budgetMonth);
        void UpdateMonthTotals(int budgetMonthId);
        void UpdateAccountHistory(Account account, DateTime balanceDate, decimal balance);

        // Delete
        void Delete(int budgetMonthId);
        void Delete(BudgetItem budgetItem);
        void Delete(BudgetCategory budgetCategory);
        void Delete(AutomationCategory category);
        void Delete(Automation automation);
        void Delete(Account account);
        void Delete(Transaction transaction);
        void Delete(PiggyBank piggyBank);
        void DeleteSplitTransactions(Transaction transaction);
        BudgetMonth ResetMonthToDefault(BudgetMonth budgetMonth, Guid user);
        void RejectChanges();
    }
}
