using DataAccess.Data;
using DataAccess.Models;

namespace DataAccess.Services
{
    public class BudgetDataService : IBudgetDataService
    {
        private readonly ApplicationDbContext _db;
        public event IBudgetDataService.NotifyBudgetDataChange BudgetDataChanged;
        public event IBudgetDataService.NotifyAccountDataChange AccountDataChanged;

        public BudgetDataService(ApplicationDbContext db)
        {
            _db = db;
        }

        #region Create
        public BudgetMonth Create(int year, int month, Guid user)
        {
            // Create a new, empty, month
            BudgetMonth newMonth = new BudgetMonth(year, month, user);

            // Add it to the database
            _db.BudgetMonths.Add(newMonth);
            _db.SaveChanges();

            // Trigger the updated event
            RaiseBudgetDataChanged();

            // Return the new month
            return newMonth;
        }

        public BudgetMonth CreateFromDefault(int year, int month, Guid user)
        {
            // Get the default month and create a new blank month
            BudgetMonth defaultMonth = GetDefaultMonth(user);
            BudgetMonth newMonth = new BudgetMonth(year, month, user);

            // Copy each category from default
            foreach (BudgetCategory category in defaultMonth.BudgetCategories)
            {
                // Create a new copy of the category
                BudgetCategory newCategory = new BudgetCategory(category.Name, category.Color);

                // Copy each item in the category from default
                foreach (BudgetItem item in category.BudgetItems)
                {
                    // Create a new copy of the item
                    BudgetItem newItem = new BudgetItem(item.Name);

                    // Add the copy to the new category
                    newCategory.BudgetItems.Add(newItem);
                }

                // Add the copy to the new month
                newMonth.BudgetCategories.Add(newCategory);
            }


            // Add it to the database
            _db.BudgetMonths.Add(newMonth);
            _db.SaveChanges();

            // Trigger the updated event
            RaiseBudgetDataChanged();

            // Return the new month
            return newMonth;
        }

        public List<Account> CreateAccount(Account account, Guid user)
        {
            // Update the account with the given user and last updated time
            account.User = user;
            account.LastUpdated = DateTime.Now;

            // Add the account to the database
            _db.Accounts.Add(account);
            _db.SaveChanges();

            // Notify subscribers that account data has changed
            RaiseAccountDataChanged();

            return _db.Accounts.Where(a => a.User == user).ToList();
        }
        #endregion

        #region Get
        public BudgetMonth Get(int budgetMonthId, Guid user)
        {
            BudgetMonth dbMonth = _db.Find<BudgetMonth>(budgetMonthId);
            return (dbMonth.User == user) ? dbMonth : null;
        }

        public BudgetMonth Get(int year, int month, Guid user)
        {
            return _db.BudgetMonths.FirstOrDefault(x => x.Year == year && x.Month == month && x.User == user);
        }

        public BudgetMonth GetOrCreate(int year, int month, Guid user)
        {
            BudgetMonth m = _db.BudgetMonths.FirstOrDefault(x => x.Year == year && x.Month == month && x.User == user);

            if (m == default(BudgetMonth))
            {
                // Current month doesn't exist, create a new one
                m = CreateFromDefault(year, month, user);
            }

            return m;
        }

        public List<BudgetMonth> GetAllMonths(Guid user)
        {
            return _db.BudgetMonths.Where(x => x.User == user).ToList();
        }

        public List<Account> GetAllAccounts(Guid user)
        {
            return _db.Accounts.Where(x => x.User == user).ToList();
        }

        public BudgetMonth GetDefaultMonth(Guid user)
        {
            // Check for the default month (month 0, year 0)
            BudgetMonth m = _db.BudgetMonths.FirstOrDefault(x => x.Year == 0 && x.Month == 0 && x.User == user);

            if (m == default(BudgetMonth))
            {
                // Default month doesn't exist yet, create it
                m = Create(0, 0, user);
            }

            return m;
        }

        public Account GetAccount(int accountId, Guid user)
        {
            return _db.Accounts.FirstOrDefault(x => x.Id == accountId && x.User == user);
        }

        public List<BudgetItem> GetBudgetItems(int year, int month, Guid user)
        {
            BudgetMonth m = Get(year, month, user);
            if (m != default(BudgetMonth))
            {
                return m.BudgetCategories.SelectMany(m => m.BudgetItems).ToList();
            }
            else
            {
                return null;
            }
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

            // TODO: Add the total spent in transactions
            //foreach (Transaction transaction in i.Transactions)
            //{
            //    totalSpent += transaction.Amount;
            //}

            i.Spent = totalSpent;
            i.Remaining = i.Budget - totalSpent;

            _db.SaveChanges();
        }

        public Account UpdateAccount(Account account)
        {
            Account a = _db.Find<Account>(account.Id);

            if (a != default(Account))
            {
                // Update the account's data
                a.LastUpdated = DateTime.Now;
                a.AccountNumber = account.AccountNumber;
                a.AccountType = account.AccountType;
                a.CurrentBalance = account.CurrentBalance;
                a.Name = account.Name;

                // Save the changes
                _db.SaveChanges();

                // Notify subscribers than an account has been updated
                RaiseAccountDataChanged();
            }

            return a;
        }
        #endregion

        #region Delete
        public void Delete(int budgetMonthId)
        {
            // Get the month to delete
            BudgetMonth budgetMonth = _db.Find<BudgetMonth>(budgetMonthId);

            // Delete the month's budget items
            _db.RemoveRange(budgetMonth.BudgetCategories.SelectMany(c => c.BudgetItems));

            // Delete the month's categories
            _db.RemoveRange(budgetMonth.BudgetCategories);

            // Delete the month
            _db.Remove(budgetMonth);

            // Save the changes
            _db.SaveChanges();

            // Trigger the updated event
            RaiseBudgetDataChanged();
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

        public BudgetMonth ResetMonthToDefault(BudgetMonth budgetMonth, Guid user)
        {
            // Delete the month
            Delete(budgetMonth.Id);

            // Return a newly created month
            return CreateFromDefault(budgetMonth.Year, budgetMonth.Month, user);
        }

        public void DeleteAccount(Account account)
        {
            // Delete the account's transactions
            _db.RemoveRange(account.Transactions);

            // Delete the account
            _db.Remove(account);
            _db.SaveChanges();

            // TODO: Update total calculations because transactions were removed?

            // Notify subscribers that account data has changed
            RaiseAccountDataChanged();
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
        /// Raises the account data changed event
        /// </summary>
        private void RaiseAccountDataChanged()
        {
            AccountDataChanged?.Invoke();
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
