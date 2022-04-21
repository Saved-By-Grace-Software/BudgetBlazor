namespace DataAccess.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Budget { get; set; }

        public List<Transaction> Transactions { get; set; }

        #region Calculated Getters
        public decimal Spent { get; set; }

        public decimal Remaining { get; set; }
        #endregion

        public BudgetItem(string name)
        {
            Transactions = new List<Transaction>();
            Name = name;

            // DEBUG - Fix when real data in EF
            Random random = new Random();
            Budget = (decimal)random.NextDouble() * Math.Abs((300) - (100)) + 100;
            Spent = (decimal)random.NextDouble() * Math.Abs(Budget);
            Remaining = Budget - Spent;
            // END DEBUG
        }

        #region Calculation Helpers
        /// <summary>
        /// Updates the Spent amount for this budget item
        /// </summary>
        public void UpdateSpent()
        {
            // DEBUG - Commenting this out so it doesn't override the hardcoded test value
            //decimal total = 0;
            //foreach (Transaction transaction in Transactions)
            //{
            //    total += transaction.Amount;
            //}
            //Spent = total;
        }

        /// <summary>
        /// Updates the Remaining amount for this budget item
        /// </summary>
        public void UpdateRemaining()
        {
            // Update the Spent amount first
            UpdateSpent();

            Remaining = Budget - Spent;
        }
        #endregion
    }
}
