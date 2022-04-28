using DataAccess.Services;
using OfxSharp;
using BudgetAccount = DataAccess.Models.Account;
using BudgetTransaction = DataAccess.Models.Transaction;

namespace BudgetBlazor.Helpers
{
    public static class TransactionImporter
    {
        public static bool ImportTransactions(string transactionsFile, BudgetAccount account, IBudgetDataService dataService)
        {
            bool success = false;

            OFXDocumentParser parser = new OFXDocumentParser();
            OFXDocument ofxDocument = parser.Import(transactionsFile);

            if (ofxDocument != null)
            {
                // Update the account balance
                account.CurrentBalance = ofxDocument.Balance.LedgerBalance;
                account.LastUpdated = ofxDocument.StatementEnd;

                // Import the transactions
                foreach(Transaction transaction in ofxDocument.Transactions)
                {
                    // Create a new budget transaction
                    BudgetTransaction t = new BudgetTransaction(transaction.Name, account.User);
                    t.Amount = transaction.Amount;
                    t.CheckNumber = transaction.CheckNum;
                    t.FITransactionId = transaction.TransactionId;
                    t.TransactionDate = transaction.Date;

                    // Add the transaction to the account
                    account.Transactions.Add(t);
                }

                // Update the account in the database
                dataService.UpdateAccount(account);

                success = true;
            }

            return success;
        }
    }
}
