using DataAccess.Models;
using DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace BudgetBlazor.Pages.Page_Components
{
    public class EditTransactionDialogBase : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }
        [Inject] IBudgetDataService BudgetDataService { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Parameter] public Transaction Transaction { get; set; }

        protected DateTime? _transactionDateBinder { get; set; }
        protected List<BudgetItem> _parentBudgets { get; set; }
        protected Guid _currentUserId { get; set; }
        protected Dictionary<Transaction, List<BudgetItem>> _splitBudgets { get; set; }
        protected Dictionary<Transaction, DateTime?> _splitDateBinders { get; set; }

        protected MudForm form;
        protected bool success;
        protected string[] errors = { };
        protected bool _isSplitsError = false;
        protected readonly string _splitsErrorMessage = "Splits must add to total";

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _transactionDateBinder = Transaction.TransactionDate;
            _parentBudgets = BudgetDataService.GetBudgetItems(Transaction.TransactionDate.Year, Transaction.TransactionDate.Month, _currentUserId);


            _splitBudgets = new Dictionary<Transaction, List<BudgetItem>>();
            _splitDateBinders = new Dictionary<Transaction, DateTime?>();
            foreach (Transaction split in Transaction.Splits)
            {
                _splitBudgets.TryAdd(split, BudgetDataService.GetBudgetItems(split.TransactionDate.Year, split.TransactionDate.Month, _currentUserId));
                _splitDateBinders.TryAdd(split, new DateTime?(split.TransactionDate));
            }
        }

        protected async Task Submit()
        {
            // If income or not a split, clear any splits that may have been created before
            if (Transaction.IsIncome || !Transaction.IsSplit)
            {
                BudgetDataService.DeleteSplitTransactions(Transaction);
                Transaction.Splits.Clear();
            }

            // Validate the form
            await form.Validate();

            // Manually validate the form along with standard validation
            if (IsSplitsAmountsCorrect() && form.IsValid)
            {
                // Update the transaction date from the binder
                Transaction.TransactionDate = (DateTime)_transactionDateBinder;

                // Clear the budget if the selected budget is not from the list, or if this is income/split
                if (!_parentBudgets.Contains(Transaction.Budget) || Transaction.IsIncome || Transaction.IsSplit)
                {
                    Transaction.Budget = null;
                }

                // If split, remove budget from parent and update the splits
                if (Transaction.IsSplit)
                {
                    // Check each split budget and update the date
                    foreach (Transaction t in Transaction.Splits)
                    {
                        if (!_splitBudgets[t].Contains(t.Budget))
                        {
                            t.Budget = null;
                        }

                        t.TransactionDate = (DateTime)_splitDateBinders[t].Value;
                    }
                }

                // Close the modal and pass back the updated transaction
                MudDialog.Close(Transaction);
            }
        }

        protected void Cancel()
        {
            // Close the dialog
            MudDialog.Cancel();
        }

        /// <summary>
        /// Reloads the budget items for the newly selected month
        /// </summary>
        protected void MonthChanged()
        {
            if (((DateTime)_transactionDateBinder).Year != Transaction.TransactionDate.Year || ((DateTime)_transactionDateBinder).Month != Transaction.TransactionDate.Month)
            {
                // Clear the existing budget
                Transaction.Budget = null;

                // The date changed to a new month, reload the budget items
                _parentBudgets = BudgetDataService.GetBudgetItems(((DateTime)_transactionDateBinder).Year, ((DateTime)_transactionDateBinder).Month, _currentUserId);
            }
        }

        /// <summary>
        /// Reloads the budget items for the newly selected month of the split
        /// </summary>
        /// <param name="split"></param>
        protected void SplitMonthChanged(Transaction split)
        {
            // Check for an existing entry
            if (_splitBudgets.ContainsKey(split))
            {
                if (((DateTime)_splitDateBinders[split]).Year != split.TransactionDate.Year || ((DateTime)_splitDateBinders[split]).Month != split.TransactionDate.Month)
                {
                    // Clear the existing budget
                    split.Budget = null;

                    // Update the existing entry
                    _splitBudgets[split] = BudgetDataService.GetBudgetItems(((DateTime)_splitDateBinders[split]).Year, ((DateTime)_splitDateBinders[split]).Month, _currentUserId);
                }   
            }
            else
            {
                // No existing entry, add one
                _splitBudgets.TryAdd(split, BudgetDataService.GetBudgetItems(((DateTime)_splitDateBinders[split]).Year, ((DateTime)_splitDateBinders[split]).Month, _currentUserId));
            }
        }

        /// <summary>
        /// Adds a new split to the transaction splits (does not save to the database here)
        /// </summary>
        /// <returns></returns>
        protected async Task AddNewTransactionSplit()
        {
            // Create a new transaction
            Transaction transactionToAdd = new Transaction(Transaction.Name, _currentUserId, Transaction.TransactionDate);

            // Add the transaction to the splits
            Transaction.Splits.Add(transactionToAdd);

            // Update the splits dictionaries
            _splitBudgets.TryAdd(transactionToAdd, BudgetDataService.GetBudgetItems(transactionToAdd.TransactionDate.Year, transactionToAdd.TransactionDate.Month, _currentUserId));
            _splitDateBinders.TryAdd(transactionToAdd, new DateTime?(transactionToAdd.TransactionDate));
        }

        /// <summary>
        /// Deletes the specified split from the transaction (does not remove it from the database here)
        /// </summary>
        /// <param name="splitToDelete"></param>
        /// <returns></returns>
        protected async Task DeleteTransactionSplit(Transaction splitToDelete)
        {
            // Remove the split from the transaction splits
            Transaction.Splits.Remove(splitToDelete);
        }

        /// <summary>
        /// Verifies that the split amounts add up to the total transaction amount
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected void VerifySplitAmounts(string arg)
        {
            _isSplitsError = !IsSplitsAmountsCorrect();
        }

        /// <summary>
        /// Checks to see if the split amounts add to the total
        /// </summary>
        /// <returns></returns>
        private bool IsSplitsAmountsCorrect()
        {
            bool ret = false;

            // Check that all of the split amounts add up to the total
            decimal totalSplits = 0;
            foreach (Transaction split in Transaction.Splits)
            {
                totalSplits += split.Amount;
            }

            if (Transaction.Splits.Count == 0 || totalSplits == Transaction.Amount)
            {
                ret = true;
            }

            return ret;
        }
    }
}
