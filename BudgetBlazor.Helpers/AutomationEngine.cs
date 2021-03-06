using BudgetBlazor.DataAccess.Models;
using BudgetBlazor.DataAccess.Services;
using MudBlazor;

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
        public static async Task<int> ExecuteSingleAutomation(
            Automation automationToExecute, Guid user, IBudgetDataService dataService, ISnackbar snackbar, List<Transaction> userTransactions = null, bool showSnackbar = false)
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
                    if (CheckRule(transaction.Name, rule))
                    {
                        // Transaction is a match, is this income?
                        if(automationToExecute.SetToIncome)
                        {
                            // Set the transaction to income
                            transaction.IsIncome = true;
                            transaction.Budget = null;
                            dataService.Update(transaction);
                            updateCounter++;
                        }
                        else
                        {
                            // Find the budget that matches the selected budget in the transaction date
                            BudgetItem item = dataService.GetMatchingBudgetItem(
                                                                automationToExecute.DefaultBudgetToSet,
                                                                transaction.TransactionDate.Month,
                                                                transaction.TransactionDate.Year,
                                                                user);

                            // Set the budget of the transaction
                            if (item != default(BudgetItem))
                            {
                                // Set the transaction budget and update it
                                transaction.Budget = item;
                                transaction.IsIncome = false;
                                dataService.Update(transaction);
                                updateCounter++;
                            }
                        } 
                    }
                }
            }

            if (showSnackbar)
            {
                snackbar.Add("Done! \"" + automationToExecute.Name + "\" updated " + updateCounter + " transactions");
            }

            return updateCounter;
        }

        /// <summary>
        /// Checks all transactions for that user in the database and runs all automations against them
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataService"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAllAutomations(Guid user, IBudgetDataService dataService, ISnackbar snackbar, bool showSnackbar = false)
        {
            int updateCounter = 0;

            // Get all transactions for this user
            List<Transaction> userTransactions = dataService.GetTransactions(user);

            // Get all automations for this user
            List<Automation> userAutomations = dataService.GetAutomations(user);

            // Process each automation
            foreach (Automation automation in userAutomations)
            {
                updateCounter += ExecuteSingleAutomation(automation, user, dataService, snackbar, userTransactions).Result;
            }

            if (showSnackbar)
            {
                snackbar.Add("Done! Updated " + updateCounter + " transactions");
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
        public static async Task<Transaction> ExecuteAllAutomations(Transaction transaction, Guid user, IBudgetDataService dataService)
        {
            // Get all automations for this user
            List<Automation> userAutomations = dataService.GetAutomations(user);

            // Check each automation against the transaction
            foreach (Automation automation in userAutomations)
            {
                // Check each automation rule for a match
                foreach (AutomationRule rule in automation.Rules)
                {
                    if (CheckRule(transaction.Name, rule))
                    {
                        // Transaction is a match, is this income?
                        if (automation.SetToIncome)
                        {
                            // Set the transaction to income
                            transaction.IsIncome = true;
                            transaction.Budget = null;
                        }
                        else
                        {
                            // Find the budget that matches the selected budget in the transaction date
                            BudgetItem item = dataService.GetMatchingBudgetItem(
                                                            automation.DefaultBudgetToSet,
                                                            transaction.TransactionDate.Month,
                                                            transaction.TransactionDate.Year,
                                                            user);

                            // Set the budget of the transaction
                            if (item != default(BudgetItem))
                            {
                                // Set the transaction budget and update it
                                transaction.Budget = item;
                                transaction.IsIncome = false;
                            }
                        }
                    }
                }
            }

            return transaction;
        }

        /// <summary>
        /// Checks to see if the transaction contains/equals the rule text
        /// </summary>
        /// <param name="transactionName"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool CheckRule(string transactionName, AutomationRule rule)
        {
            bool ret;

            if (rule.IsExactMatch)
            {
                ret = transactionName.Trim().Equals(rule.ContainsText);
            }
            else
            {
                ret = transactionName.Contains(rule.ContainsText, StringComparison.CurrentCultureIgnoreCase);
            }

            return ret;
        }
    }
}
