﻿namespace DataAccess.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Budget { get; set; }

        public List<Transaction> Transactions { get; set; }

        public decimal Spent { get; set; }

        public decimal Remaining { get; set; }

        public BudgetItem(string name)
        {
            Transactions = new List<Transaction>();
            Name = name;
        }

        #region Calculation Helpers
        /// <summary>
        /// Updates the Spent amount for this budget item
        /// </summary>
        public void UpdateSpent()
        {
            decimal total = 0;
            foreach (Transaction transaction in Transactions)
            {
                total += transaction.Amount;
            }
            Spent = total;
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
