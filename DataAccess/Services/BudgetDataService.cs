using DataAccess.Data;
using DataAccess.Models;

namespace DataAccess.Services
{
    public class BudgetDataService : IBudgetDataService
    {
        private readonly ApplicationDbContext _db;
        public event IBudgetDataService.NotifyDataChange BudgetDataChanged;

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
            return _db.Find<BudgetMonth>(budgetMonthId);
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
            BudgetMonth m = _db.Find<BudgetMonth>(budgetMonth.Id);

            if (m != default(BudgetMonth))
            {
                // Update the month's user specified data
                m.BudgetCategories = budgetMonth.BudgetCategories;
                m.ExpectedIncome = budgetMonth.ExpectedIncome;
                _db.SaveChanges();

                // Update the month's calculated totals
                UpdateMonthTotals(m);

                // Save the changes
                _db.SaveChanges();

                // Notify subscribers that the budget data been updated
                RaiseBudgetDataChanged();
            }

            // This will return null if trying to update a month that doesn't exist
            return m;
        }

        public BudgetCategory Update(BudgetCategory budgetCategory)
        {
            // Get the category in the database
            BudgetCategory c = _db.Find<BudgetCategory>(budgetCategory.Id);

            if (c != default(BudgetCategory))
            {
                // Update the category's user specified data
                c.Budgeted = budgetCategory.Budgeted;
                c.BudgetItems = budgetCategory.BudgetItems;
                c.Color = budgetCategory.Color;
                c.Name = budgetCategory.Name;
                _db.SaveChanges();

                // Update the month's (parent) calculated totals
                BudgetMonth parent = GetMonthFromCategory(c);
                UpdateMonthTotals(parent);

                // Save the changes
                _db.SaveChanges();

                // Trigger the updated event
                RaiseBudgetDataChanged();
            }

            return c;
        }

        public BudgetItem Update(BudgetItem budgetItem)
        {
            BudgetItem i = _db.Find<BudgetItem>(budgetItem.Id);

            if (i != default(BudgetItem))
            {
                // Update the item's user specified data
                i.Name = budgetItem.Name;
                i.Budget = budgetItem.Budget;
                _db.SaveChanges();

                // Update the month's (grandparent) calculated totals
                BudgetMonth grandparent = GetMonthFromItem(i);
                UpdateMonthTotals(grandparent);

                // Save the changes
                _db.SaveChanges();

                // Trigger the updated event
                RaiseBudgetDataChanged();
            }

            return i;
        }

        /// <summary>
        /// Updates the totals for the given month
        /// </summary>
        /// <param name="budgetMonth"></param>
        public void UpdateMonthTotals(BudgetMonth budgetMonth)
        {
            UpdateMonthTotals(budgetMonth.Id);
        }

        /// <summary>
        /// Updates the totals for the given month
        /// </summary>
        /// <param name="budgetMonthId"></param>
        public void UpdateMonthTotals(int budgetMonthId)
        {
            decimal totalBudgeted = 0;
            decimal totalSpent = 0;
            BudgetMonth m = _db.Find<BudgetMonth>(budgetMonthId);

            // Update each category
            foreach (BudgetCategory category in m.BudgetCategories)
            {
                UpdateCategoryTotals(category);

                totalBudgeted += category.Budgeted;
                totalSpent += category.Spent;
            }

            m.TotalBudgeted = totalBudgeted;
            m.TotalSpent = totalSpent;

            _db.SaveChanges();
        }

        /// <summary>
        /// Updates the totals for the given category
        /// </summary>
        /// <param name="budgetCategory"></param>
        private void UpdateCategoryTotals(BudgetCategory budgetCategory)
        {
            decimal totalBudgeted = 0;
            decimal totalSpent = 0;
            BudgetCategory c = _db.Find<BudgetCategory>(budgetCategory.Id);

            // Update each budget
            foreach (BudgetItem item in c.BudgetItems)
            {
                UpdateBudgetItemTotals(item);

                totalBudgeted += item.Budget;
                totalSpent += item.Spent;
            }

            c.Budgeted = totalBudgeted;
            c.Spent = totalSpent;
            c.Remaining = totalBudgeted - totalSpent;

            _db.SaveChanges();
        }

        /// <summary>
        /// Updates the totals for the given budget item
        /// </summary>
        /// <param name="budgetItem"></param>
        private void UpdateBudgetItemTotals(BudgetItem budgetItem)
        {
            decimal totalSpent = 0;
            BudgetItem i = _db.Find<BudgetItem>(budgetItem.Id);

            // Add the total spent in transactions
            foreach (Transaction transaction in i.Transactions)
            {
                totalSpent += transaction.Amount;
            }

            i.Spent = totalSpent;
            i.Remaining = i.Budget - totalSpent;

            _db.SaveChanges();
        }
        #endregion

        #region Delete
        public void Delete(int budgetMonthId)
        {
            throw new NotImplementedException();
        }

        public void Delete(BudgetItem budgetItem)
        {
            BudgetMonth grandparent = GetMonthFromItem(budgetItem);

            // Delete the budget item
            _db.Remove(budgetItem);
            _db.SaveChanges();

            // Update the month's (grandparent) calculated totals
            UpdateMonthTotals(grandparent);

            // Save the changes
            _db.SaveChanges();

            // Trigger the updated event
            RaiseBudgetDataChanged();
        }

        public void Delete(BudgetCategory budgetCategory)
        {
            BudgetMonth parent = GetMonthFromCategory(budgetCategory);

            // Delete the category's budget items
            _db.RemoveRange(budgetCategory.BudgetItems);

            // Delete the category from the parent month
            _db.Remove(budgetCategory);
            _db.SaveChanges();

            // Update the month's (parent) calculated totals
            UpdateMonthTotals(parent);

            // Save the changes
            _db.SaveChanges();

            // Trigger the updated event
            RaiseBudgetDataChanged();
        }
        #endregion

        #region Private Helper Functions
        /// <summary>
        /// Raises the budget data changed event
        /// </summary>
        private void RaiseBudgetDataChanged()
        {
            BudgetDataChanged?.Invoke();
        }

        /// <summary>
        /// Gets the parent month from the given category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private BudgetMonth? GetMonthFromCategory(BudgetCategory category)
        {
            // Get the month ID from the item's parent
            int monId = (int)_db.Entry(category).Property("BudgetMonthId").CurrentValue;

            // Get the month from the db based on the ID
            return _db.Find<BudgetMonth>(monId);
        }

        /// <summary>
        /// Gets the grandparent month from the given item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private BudgetMonth? GetMonthFromItem(BudgetItem item)
        {
            // Get the category ID from the item's parent
            int catId = (int)_db.Entry(item).Property("BudgetCategoryId").CurrentValue;

            // Get the month from the db based on the category
            return GetMonthFromCategory(_db.Find<BudgetCategory>(catId));
        }
        #endregion
    }
}
