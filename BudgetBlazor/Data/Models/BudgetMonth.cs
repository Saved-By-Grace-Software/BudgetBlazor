namespace BudgetBlazor.Data.Models
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

        #region Calculation Getters
        public decimal ActualIncome { get; }

        public decimal TotalBudgeted { get; }

        public decimal TotalSpent { get; }

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
            ExpectedIncome = 1000 + (Month * 100);
            Random random = new Random();
            TotalBudgeted = (decimal)random.NextDouble() * Math.Abs(ExpectedIncome);
            ActualIncome = (decimal)random.NextDouble() * Math.Abs((ExpectedIncome + 50) - (ExpectedIncome - 50)) + ExpectedIncome;
            TotalSpent = ((decimal)random.NextDouble() * Math.Abs(ActualIncome));

            int numCategories = random.Next(6);
            for (int i = 0; i < numCategories; i++)
            {
                BudgetCategories.Add(new BudgetCategory("Category " + i));
            }
            // END DEBUG
        }
    }
}
