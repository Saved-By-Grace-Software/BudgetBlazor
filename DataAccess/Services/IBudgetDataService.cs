using DataAccess.Models;

namespace DataAccess.Services
{
    public interface IBudgetDataService
    {
        // Event to notify subscribers when changes occur
        public delegate void NotifyDataChange();
        public event NotifyDataChange BudgetDataChanged;

        // Create
        BudgetMonth Create(int year, int month, Guid user);

        // Get
        BudgetMonth Get(int budgetMonthId, Guid user);
        BudgetMonth Get(int year, int month, Guid user);
        BudgetMonth GetOrCreate(int year, int month, Guid user);
        List<BudgetMonth> GetAll(Guid user);
        BudgetMonth GetDefaultMonth(Guid user);

        // Update
        BudgetMonth Update(BudgetMonth budgetMonth);
        BudgetCategory Update(BudgetCategory budgetCategory);
        BudgetItem Update(BudgetItem budgetItem);
        void UpdateMonthTotals(BudgetMonth budgetMonth);
        void UpdateMonthTotals(int budgetMonthId);

        // Delete
        void Delete(int budgetMonthId);
        void Delete(BudgetItem budgetItem);
        void Delete(BudgetCategory budgetCategory);
    }
}
