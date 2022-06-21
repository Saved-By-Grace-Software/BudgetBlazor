using BudgetBlazor.DataAccess.Services;
using CsvHelper;
using OfxSharp;
using System.Globalization;
using BudgetAccount = BudgetBlazor.DataAccess.Models.Account;
using BudgetTransaction = BudgetBlazor.DataAccess.Models.Transaction;

namespace BudgetBlazor.Helpers
{
    public static class TransactionImporter
    {
        /// <summary>
        /// Imports transactions from an OFX file
        /// </summary>
        /// <param name="transactionsFile">String buffer containing file contents</param>
        /// <param name="account">Account to import the transactions into</param>
        /// <param name="dataService">Data service for adding transactions into the database</param>
        /// <returns></returns>
        public static async Task<bool> ImportOFXTransactions(string transactionsFile, BudgetAccount account, IBudgetDataService dataService)
        {
            bool success = false;

            OFXDocumentParser parser = new OFXDocumentParser();

            try
            {
                OFXDocument ofxDocument = parser.Import(transactionsFile);

                if (ofxDocument != null)
                {
                    // Update the account balance if this import is more recent than the last update
                    if (ofxDocument.StatementEnd > account.LastUpdated)
                    {
                        account.CurrentBalance = ofxDocument.Balance.LedgerBalance;
                        account.LastUpdated = ofxDocument.StatementEnd;
                    }

                    // Import the transactions
                    foreach (Transaction transaction in ofxDocument.Transactions)
                    {
                        // Create a new budget transaction
                        BudgetTransaction t = new BudgetTransaction(transaction.Name, account.User);
                        t.Amount = transaction.Amount;
                        t.CheckNumber = transaction.CheckNum;
                        t.FITransactionId = transaction.TransactionId;
                        t.TransactionDate = transaction.Date;

                        // Add the transaction to the account if it is unique
                        if (!DoesTransactionExist(account.Transactions, t))
                        {
                            // Run automations against the transaction
                            t = await AutomationEngine.ExecuteAllAutomations(t, account.User, dataService);

                            // Add the transaction to the account
                            account.Transactions.Add(t);
                        }
                    }

                    // Update the account in the database
                    dataService.Update(account);

                    // Add the account history to the database
                    dataService.UpdateAccountHistory(account, ofxDocument.StatementEnd, ofxDocument.Balance.LedgerBalance);

                    success = true;
                }
            }
            catch (Exception)
            {
                // Failed to parse transactions, return false
            }

            return success;
        }

        /// <summary>
        /// Imports transactions from a CSV file
        /// </summary>
        /// <param name="transactionsFile">String buffer containing file contents</param>
        /// <param name="account">Account to import the transactions into</param>
        /// <param name="dataService">Data service for adding transactions into the database</param>
        /// <returns></returns>
        public static async Task<bool> ImportCSVTransactions(string transactionsFile, BudgetAccount account, IBudgetDataService dataService)
        {
            bool success = false;
            List<dynamic> records = new List<dynamic>();

            try
            {
                // Parse the csv records into a list
                using (var csv = new CsvReader(new StringReader(transactionsFile), CultureInfo.InvariantCulture))
                {
                    records = csv.GetRecords<dynamic>().ToList();
                }

                // Temp variables to store the most recent balance
                DateTime latestBalanceDate = DateTime.MinValue;
                decimal latestBalance = 0;

                // Iterate through the records to create transactions
                foreach (IDictionary<string, object> record in records)
                {
                    // Get the data from the record
                    string transactionName = (string)record["Details"];

                    if (DateTime.TryParse((string)record["Date"], out DateTime transactionDate) &&
                        decimal.TryParse((string)record["Amount"], NumberStyles.Currency, null, out decimal transactionAmount))
                    {
                        // Create a new budget transaction
                        BudgetTransaction t = new BudgetTransaction(transactionName, account.User);
                        t.Amount = transactionAmount;
                        t.TransactionDate = transactionDate;

                        // Add the transaction to the account if it is unique
                        if (!DoesTransactionExist(account.Transactions, t))
                        {
                            // Run automations against the transaction
                            t = await AutomationEngine.ExecuteAllAutomations(t, account.User, dataService);

                            // Add the transaction to the account
                            account.Transactions.Add(t);
                        }

                        // Check if this is the latest balance to update latest balance
                        if (transactionDate > latestBalanceDate &&
                            decimal.TryParse((string)record["Balance"], NumberStyles.Currency, null, out decimal balanceAmount))
                        {
                            latestBalanceDate = transactionDate;
                            latestBalance = balanceAmount;
                        }
                    }
                }

                // Update the account balance
                account.CurrentBalance = latestBalance;
                account.LastUpdated = latestBalanceDate;

                // Update the account in the database
                dataService.Update(account);

                // Add the account history to the database
                dataService.UpdateAccountHistory(account, latestBalanceDate, latestBalance);

                success = true;
            }
            catch (Exception)
            {
                // Failed to parse transactions, return false
            }

            return success;
        }

        /// <summary>
        /// Determines if the given transaction exists in the list of transactions
        /// </summary>
        /// <param name="currentTransactions"></param>
        /// <param name="transactionToAdd"></param>
        /// <returns></returns>
        private static bool DoesTransactionExist(List<BudgetTransaction> currentTransactions, BudgetTransaction transactionToAdd)
        {
            // Default to true so we fail to not adding the transaction
            bool ret = true;

            if (transactionToAdd.FITransactionId == null || string.IsNullOrWhiteSpace(transactionToAdd.FITransactionId))
            {
                ret = currentTransactions.Contains(transactionToAdd);
            }
            else
            {
                ret = currentTransactions.FirstOrDefault(x => x.FITransactionId == transactionToAdd.FITransactionId) != default(BudgetTransaction);
            }

            return ret;
        }
    }
}
