using DataAccess.Models;

namespace BudgetBlazor.Services
{
    public class BudgetMonthService : IBudgetMonthService
    {
        public BudgetMonth Create(BudgetMonth budgetMonth)
        {
            throw new NotImplementedException();
        }

        public void Delete(int budgetMonthId)
        {
            throw new NotImplementedException();
        }

        public BudgetMonth Get(int budgetMonthId)
        {
            throw new NotImplementedException();
        }

        public BudgetMonth Get(int year, int month)
        {
            // DEBUG - Remove when EF connected to the database
            Random random = new Random();
            BudgetMonth budgetMonth = new BudgetMonth(year, month);

            int numCategories = random.Next(4);
            for (int j = 0; j < numCategories; j++)
            {
                BudgetCategory category = new BudgetCategory("Category " + j);

                int numItems = random.Next(5);
                for (int k = 0; k < numItems; k++)
                {
                    BudgetItem item = new BudgetItem("Budget " + k);
                    item.Budget = (decimal)random.NextDouble() * Math.Abs((300) - (100)) + 100;
                    item.Spent = (decimal)random.NextDouble() * Math.Abs(item.Budget);
                    item.Remaining = item.Budget - item.Spent;
                    category.BudgetItems.Add(item);
                }

                category.UpdateCategoryTotals();
                budgetMonth.BudgetCategories.Add(category);
            }

            budgetMonth.ExpectedIncome = 1000 + (month * 100);
            budgetMonth.ActualIncome = (decimal)random.NextDouble() * Math.Abs((budgetMonth.ExpectedIncome + 50) - (budgetMonth.ExpectedIncome - 50)) + budgetMonth.ExpectedIncome;
            budgetMonth.UpdateMonthTotals();

            return budgetMonth;
            // END DEBUG
        }

        public List<BudgetMonth> GetAll()
        {
            // DEBUG - Remove when EF connected to the database
            List<BudgetMonth> list = new List<BudgetMonth>();
            Random random = new Random();

            for (int i = 1; i <= 12; i++)
            {
                BudgetMonth month = new BudgetMonth(2022, i);

                int numCategories = random.Next(4);
                for (int j = 0; j < numCategories; j++)
                {
                    BudgetCategory category = new BudgetCategory("Category " + j);

                    int numItems = random.Next(5);
                    for (int k = 0; k < numItems; k++)
                    {
                        BudgetItem item = new BudgetItem("Budget " + k);
                        item.Budget = (decimal)random.NextDouble() * Math.Abs((300) - (100)) + 100;
                        item.Spent = (decimal)random.NextDouble() * Math.Abs(item.Budget);
                        item.Remaining = item.Budget - item.Spent;
                        category.BudgetItems.Add(item);
                    }

                    category.UpdateCategoryTotals();
                    month.BudgetCategories.Add(category);
                }

                month.ExpectedIncome = 1000 + (i * 100);
                month.ActualIncome = (decimal)random.NextDouble() * Math.Abs((month.ExpectedIncome + 50) - (month.ExpectedIncome - 50)) + month.ExpectedIncome;
                month.UpdateMonthTotals();
                list.Add(month);
            }

            return list;
            // END DEBUG
        }

        public BudgetMonth Update(BudgetMonth budgetMonth)
        {
            throw new NotImplementedException();
        }
    }
}
