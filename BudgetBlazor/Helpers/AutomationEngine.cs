using DataAccess.Models;
using DataAccess.Services;

namespace BudgetBlazor.Helpers
{
    public static class AutomationEngine
    {
        public static async Task<int> ExecuteSingleAutomation(
            Automation automationToExecute, Guid user, IBudgetDataService dataService, List<Transaction> userTransactions = null)
        {
            int updateCounter = 0;

            // Get all transactions for this user
            if (userTransactions == null)
            {
                userTransactions = dataService.GetTransactions(user);
            }

            if (userTransactions == null)
                return updateCounter;

            // Check each transaction to see if it is a match
            foreach(Transaction transaction in userTransactions)
            {
                // Check each automation rule for a match
                foreach(AutomationRule rule in automationToExecute.Rules)
                {
                    if (transaction.Name.Contains(rule.ContainsText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Transaction is a match, find the budget that matches the selected budget in the transaction date
                        BudgetItem item = dataService.GetMatchingBudgetItem(
                                                            automationToExecute.DefaultBudgetToSet, 
                                                            transaction.TransactionDate.Month, 
                                                            transaction.TransactionDate.Year, 
                                                            user);

                        if (item != default(BudgetItem))
                        {
                            // Set the transaction budget and update it
                            transaction.Budget = item;
                            dataService.Update(transaction);
                            updateCounter++;
                        }
                    }
                }
            }

            return updateCounter;
        }

        public static async Task<int> ExecuteAllAutomations(Guid user, IBudgetDataService dataService)
        {
            int updateCounter = 0;

            // Get all transactions for this user
            List<Transaction> userTransactions = dataService.GetTransactions(user);

            // Get all automations for this user
            List<Automation> userAutomations = dataService.GetAutomations(user);

            // Process each automation
            foreach (Automation automation in userAutomations)
            {
                updateCounter += await ExecuteSingleAutomation(automation, user, dataService, userTransactions);
            }

            return updateCounter;
        }
    }
}
