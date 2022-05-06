using DataAccess.Models;
using DataAccess.Services;

namespace BudgetBlazor.Helpers
{
    public static class AutomationEngine
    {
        /// <summary>
        /// Checks all transactions in the database for that user and runs the automation against them
        /// </summary>
        /// <param name="automationToExecute"></param>
        /// <param name="user"></param>
        /// <param name="dataService"></param>
        /// <param name="userTransactions"></param>
        /// <returns></returns>
        public static int ExecuteSingleAutomation(
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

        /// <summary>
        /// Checks all transactions for that user in the database and runs all automations against them
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataService"></param>
        /// <returns></returns>
        public static int ExecuteAllAutomations(Guid user, IBudgetDataService dataService)
        {
            int updateCounter = 0;

            // Get all transactions for this user
            List<Transaction> userTransactions = dataService.GetTransactions(user);

            // Get all automations for this user
            List<Automation> userAutomations = dataService.GetAutomations(user);

            // Process each automation
            foreach (Automation automation in userAutomations)
            {
                updateCounter += ExecuteSingleAutomation(automation, user, dataService, userTransactions);
            }

            return updateCounter;
        }

        /// <summary>
        /// Executes all automations against the provided transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="user"></param>
        /// <param name="dataService"></param>
        /// <returns></returns>
        public static Transaction ExecuteAllAutomations(Transaction transaction, Guid user, IBudgetDataService dataService)
        {
            // Get all automations for this user
            List<Automation> userAutomations = dataService.GetAutomations(user);

            // Check each automation against the transaction
            foreach (Automation automation in userAutomations)
            {
                // Check each automation rule for a match
                foreach (AutomationRule rule in automation.Rules)
                {
                    if (transaction.Name.Contains(rule.ContainsText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Transaction is a match, find the budget that matches the selected budget in the transaction date
                        BudgetItem item = dataService.GetMatchingBudgetItem(
                                                            automation.DefaultBudgetToSet,
                                                            transaction.TransactionDate.Month,
                                                            transaction.TransactionDate.Year,
                                                            user);

                        if (item != default(BudgetItem))
                        {
                            // Set the transaction budget and update it
                            transaction.Budget = item;
                        }
                    }
                }
            }

            return transaction;
        }
    }
}
