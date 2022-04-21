using DataAccess.Data;
using DataAccess.Models;

namespace DataAccess.Services
{
    public class BudgetDataService : IBudgetDataService
    {
        private readonly ApplicationDbContext _db;

        public event IBudgetDataService.NotifyMonthChange BudgetMonthChanged;

        public BudgetDataService(ApplicationDbContext db)
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
                // Update the month's user specified data
                m.BudgetCategories = budgetMonth.BudgetCategories;
                m.ExpectedIncome = budgetMonth.ExpectedIncome;

                // Update the month's calculated totals
                m.UpdateMonthTotals();

                // Save the changes
                _db.SaveChanges();

                // Notify subscribers that the month has been updated
                RaiseBudgetMonthChanged(m);
            }

            // This will return null if trying to update a month that doesn't exist
            return m;
        }

        public BudgetCategory Update(BudgetCategory budgetCategory)
        {
            BudgetCategory c = _db.BudgetMonths.SelectMany(m => m.BudgetCategories).FirstOrDefault(x => x.Id == budgetCategory.Id);

            if (c != default(BudgetCategory))
            {
                // Update the category's user specified data
                c.Budgeted = budgetCategory.Budgeted;
                c.BudgetItems = budgetCategory.BudgetItems;
                c.Color = budgetCategory.Color;
                c.Name = budgetCategory.Name;

                // Update the month's (parent) calculated totals
                BudgetMonth parent = GetMonthFromCategory(c);
                parent?.UpdateMonthTotals();

                // Save the changes
                _db.SaveChanges();

                // Trigger the updated event
                RaiseBudgetMonthChanged(parent);
            }

            return c;
        }
        #endregion

        #region Delete
        public void Delete(int budgetMonthId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Helper Functions
        /// <summary>
        /// Raises the budget month changed event
        /// </summary>
        /// <param name="updatedMonth">The month that changed</param>
        private void RaiseBudgetMonthChanged(BudgetMonth updatedMonth)
        {
            if (updatedMonth != null)
            {
                BudgetMonthChanged?.Invoke(updatedMonth);
            }
        }

        /// <summary>
        /// Gets the parent month from the given category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private BudgetMonth? GetMonthFromCategory(BudgetCategory category)
        {
            return ((ApplicationDbContext)_db.Entry(category).Context).BudgetMonths.FirstOrDefault();
        }
        #endregion
    }
}
