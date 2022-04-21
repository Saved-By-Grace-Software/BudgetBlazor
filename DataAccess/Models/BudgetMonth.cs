namespace DataAccess.Models
{
    public class BudgetMonth
    {
        #region Public Data Parameters
        public int Id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public decimal ExpectedIncome { get; set; }

        public List<BudgetCategory> BudgetCategories { get; set; }

        public Guid User { get; set; }
        #endregion

        #region Calculated Getters
        public decimal ActualIncome { get; set; }

        public decimal TotalBudgeted { get; set; }

        public decimal TotalSpent { get; set; }

        public int PercentBudgeted
        {
            get
            {
                if (ExpectedIncome == 0)
                    return 0;
                else
                    return (int)(TotalBudgeted / ExpectedIncome * 100);
            }
        }

        public int PercentSpent
        {
            get
            {
                if (ActualIncome == 0)
                    return 0;
                else
                    return (int)(TotalSpent / ActualIncome * 100);
            }
        }
        #endregion

        public BudgetMonth(int year, int month)
        {
            BudgetCategories = new List<BudgetCategory>();
            Year = year;
            Month = month;

            // DEBUG - Fix when real data in EF
            Random random = new Random();
            int numCategories = random.Next(4);
            for (int i = 0; i < numCategories; i++)
            {
                BudgetCategory temp = new BudgetCategory("Category " + i);
                temp.UpdateCategoryTotals();
                BudgetCategories.Add(temp);
            }

            ExpectedIncome = 1000 + (Month * 100);
            ActualIncome = (decimal)random.NextDouble() * Math.Abs((ExpectedIncome + 50) - (ExpectedIncome - 50)) + ExpectedIncome;
            UpdateMonthTotals();
            // END DEBUG
        }

        #region Calculation Helpers
        /// <summary>
        /// Updates the totals for the month
        /// </summary>
        public void UpdateMonthTotals()
        {
            UpdateTotalBudgeted();
            UpdateTotalSpent();
        }

        /// <summary>
        /// Updates the total budgeted amount for the month
        /// </summary>
        public void UpdateTotalBudgeted()
        {
            decimal total = 0;
            foreach (BudgetCategory category in BudgetCategories)
            {
                // Update category totals
                category.UpdateBudgeted();

                // Add the total budgeted
                total += category.Budgeted;
            }
            TotalBudgeted = total;
        }

        /// <summary>
        /// Updates the total spent amount for the month
        /// </summary>
        public void UpdateTotalSpent()
        {
            decimal total = 0;
            foreach (BudgetCategory category in BudgetCategories)
            {
                // Update spent totals
                category.UpdateSpent();

                // Add the total spent
                total += category.Spent;
            }
            TotalSpent = total;
        }
        #endregion
    }
}
