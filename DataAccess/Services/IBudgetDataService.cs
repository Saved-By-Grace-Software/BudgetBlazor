using DataAccess.Models;

namespace DataAccess.Services
{
    public interface IBudgetDataService
    {
        // Event to notify subscribers when changes occur
        public delegate void NotifyMonthChange(BudgetMonth updatedMonth);
        public event NotifyMonthChange BudgetMonthChanged;

        // Create
        BudgetMonth Create(int year, int month);

        // Get
        BudgetMonth Get(int budgetMonthId);
        BudgetMonth Get(int year, int month);
        BudgetMonth GetOrCreate(int year, int month);
        List<BudgetMonth> GetAll();

        // Update
        BudgetMonth Update(BudgetMonth budgetMonth);
        BudgetCategory Update(BudgetCategory budgetCategory);

        // Delete
        void Delete(int budgetMonthId);
    }
}
