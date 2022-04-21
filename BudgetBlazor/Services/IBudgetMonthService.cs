using DataAccess.Models;

namespace BudgetBlazor.Services
{
    public interface IBudgetMonthService
    {
        // Create
        BudgetMonth Create(BudgetMonth budgetMonth);

        // Get
        BudgetMonth Get(int budgetMonthId);
        BudgetMonth Get(int year, int month);

        // GetAll
        List<BudgetMonth> GetAll();

        // Update
        BudgetMonth Update(BudgetMonth budgetMonth);

        // Delete
        void Delete(int budgetMonthId);
    }
}
