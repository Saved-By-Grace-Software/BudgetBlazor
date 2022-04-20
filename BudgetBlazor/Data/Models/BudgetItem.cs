namespace BudgetBlazor.Data.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Budget { get; set; }

        public List<Transaction> Transactions { get; set; }

        #region Calculation Getters
        public decimal Spent { get; }

        public decimal Remaining { get; }
        #endregion

        public BudgetItem(string name)
        {
            Transactions = new List<Transaction>();
            Name = name;

            // DEBUG - Fix when real data in EF
            Random random = new Random();
            Budget = (decimal)random.NextDouble() * Math.Abs((300) - (100)) + 100;
            Spent = (decimal)random.NextDouble() * Math.Abs(Budget);
            Remaining = (decimal)random.NextDouble() * Math.Abs((Budget + 50) - (Budget - 50)) + Budget;
            // END DEBUG
        }
    }
}
