using DataAccess.Data;
using DataAccess.Models;

namespace DataAccess.Services
{
    public class BudgetMonthService : IBudgetMonthService
    {
        private readonly ApplicationDbContext _db;

        public BudgetMonthService(ApplicationDbContext db)
        {
            _db = db;
        }

        #region Create
        public BudgetMonth Create(int year, int month)
        {
            // Create a new budget month
            BudgetMonth budgetMonth = new BudgetMonth(year, month);

            // Add it to the database
            _db.BudgetMonths.Add(budgetMonth);
            _db.SaveChanges();

            // Return the new month
            return budgetMonth;
        }
        #endregion

        #region Get
        public BudgetMonth Get(int budgetMonthId)
        {
            throw new NotImplementedException();
        }

        public BudgetMonth Get(int year, int month)
        {
            return _db.BudgetMonths.FirstOrDefault(x => x.Year == year && x.Month == month);
        }

        public BudgetMonth GetOrCreate(int year, int month)
        {
            BudgetMonth m = _db.BudgetMonths.FirstOrDefault(x => x.Year == year && x.Month == month);

            if (m == default(BudgetMonth))
            {
                // Current month doesn't exist, create a new one
                m = Create(year, month);
            }

            return m;
        }

        public List<BudgetMonth> GetAll()
        {
            return _db.BudgetMonths.ToList();
        }
        #endregion

        #region Update
        public BudgetMonth Update(BudgetMonth budgetMonth)
        {
            BudgetMonth m = _db.BudgetMonths.FirstOrDefault(x => x.Id == budgetMonth.Id);

            if (m != default(BudgetMonth))
            {
                // Update the month data
                m.ActualIncome = budgetMonth.ActualIncome;
                m.BudgetCategories = budgetMonth.BudgetCategories;
                m.ExpectedIncome = budgetMonth.ExpectedIncome;
                m.TotalBudgeted = budgetMonth.TotalBudgeted;
                m.TotalSpent = budgetMonth.TotalSpent;

                // Save the changes
                _db.SaveChanges();
            }

            // This will return null if trying to update a month that doesn't exist
            return m;
        }

        //public BudgetCategory Update(BudgetCategory budgetCategory)
        //{
            
        //}
        #endregion

        #region Delete
        public void Delete(int budgetMonthId)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
