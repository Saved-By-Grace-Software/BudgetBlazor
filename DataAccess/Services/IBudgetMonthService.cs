using DataAccess.Models;

namespace DataAccess.Services
{
    public interface IBudgetMonthService
    {
        // Create
        BudgetMonth Create(int year, int month);

        // Get
        BudgetMonth Get(int budgetMonthId);
        BudgetMonth Get(int year, int month);
        BudgetMonth GetOrCreate(int year, int month);
        List<BudgetMonth> GetAll();

        // Update
        BudgetMonth Update(BudgetMonth budgetMonth);

        // Delete
        void Delete(int budgetMonthId);
    }
}
