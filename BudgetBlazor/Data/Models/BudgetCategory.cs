﻿using MudBlazor;

namespace BudgetBlazor.Data.Models
{
    public class BudgetCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public List<BudgetItem> BudgetItems { get; set; }

        public decimal Budgeted { get; set; }

        public decimal Spent { get; set; }

        public decimal Remaining { get; set; }

        public BudgetCategory(string name)
        {
            BudgetItems = new List<BudgetItem>();
            Name = name;
            Color = "#1ec8a54d";

            // DEBUG - Fix when real data in EF
            Random random = new Random();
            int numItems = random.Next(5);
            for (int i=0; i < numItems; i++)
            {
                BudgetItem item = new BudgetItem("Budget " + i);
                BudgetItems.Add(item);
            }
            // END DEBUG
        }

        #region Calculation Helpers
        /// <summary>
        /// Updates the total numbers for the category
        /// </summary>
        public void UpdateCategoryTotals()
        {
            // We only need to call UpdateRemaining because internally it updates Budgeted and Spent
            UpdateRemaining();
        }

        /// <summary>
        /// Updates the Budgeted amount for this category
        /// </summary>
        public void UpdateBudgeted()
        {
            decimal total = 0;
            foreach (BudgetItem item in BudgetItems)
            {
                total += item.Budget;
            }
            Budgeted = total;
        }

        /// <summary>
        /// Updates the Spent amount for this category
        /// </summary>
        public void UpdateSpent()
        {
            decimal total = 0;
            foreach (BudgetItem item in BudgetItems)
            {
                // Update the Spent amount for this item
                item.UpdateSpent();

                // Add the udpated amount to the total
                total += item.Spent;
            }
            Spent = total;
        }

        /// <summary>
        /// Updates the Remaining amount for this category
        /// </summary>
        public void UpdateRemaining()
        {
            // Update the Budgeted and Spent amounts first
            UpdateBudgeted();
            UpdateSpent();

            Remaining = Budgeted - Spent;
        }
        #endregion
    }
}
