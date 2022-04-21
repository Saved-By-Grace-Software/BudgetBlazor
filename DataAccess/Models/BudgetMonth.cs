using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class BudgetMonth
    {
        #region Public Data Parameters
        public int Id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ExpectedIncome { get; set; }

        public List<BudgetCategory> BudgetCategories { get; set; }

        public Guid User { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ActualIncome { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalBudgeted { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalSpent { get; set; }

        [NotMapped]
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

        [NotMapped]
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
