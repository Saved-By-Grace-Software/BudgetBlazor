using BudgetBlazor.DataAccess.Models;
using BudgetBlazor.DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace BudgetBlazor.PageComponents
{
    public class EditTransactionDialogBase : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }
        [Inject] IBudgetDataService BudgetDataService { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Parameter] public Transaction Transaction { get; set; }

        protected DateTime? _transactionDateBinder { get; set; }
        protected List<BudgetCategory> _parentCategories { get; set; }
        protected Guid _currentUserId { get; set; }
        protected Dictionary<Transaction, List<BudgetCategory>> _splitCategories { get; set; }
        protected Dictionary<Transaction, DateTime?> _splitDateBinders { get; set; }

        protected MudForm form;
        protected bool success;
        protected string[] errors = { };
        protected bool _isSplitsError = false;
        protected string _splitsErrorMessage = "Splits must add to total";
        protected bool _hideRemainingSplits = true;
        protected string _remainingSplitsMessage = "";

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _transactionDateBinder = Transaction.TransactionDate;
            _parentCategories = BudgetDataService.GetBudgetCategories(Transaction.TransactionDate.Year, Transaction.TransactionDate.Month, _currentUserId);

            _splitCategories = new Dictionary<Transaction, List<BudgetCategory>>();
            _splitDateBinders = new Dictionary<Transaction, DateTime?>();
            foreach (Transaction split in Transaction.Splits)
            {
                _splitCategories.TryAdd(split, BudgetDataService.GetBudgetCategories(split.TransactionDate.Year, split.TransactionDate.Month, _currentUserId));
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
            if (IsSplitsAmountsCorrect(out decimal amountRemaining) && form.IsValid)
            {
                // Update the transaction date from the binder
                Transaction.TransactionDate = (DateTime)_transactionDateBinder;

                // Clear the budget if the selected budget is not from the list, or if this is income/split
                if (!_parentCategories.SelectMany(c => c.BudgetItems).Contains(Transaction.Budget) || Transaction.IsIncome || Transaction.IsSplit)
                {
                    Transaction.Budget = null;
                }

                // If split, remove budget from parent and update the splits
                if (Transaction.IsSplit)
                {
                    // Check each split budget and update the date
                    foreach (Transaction t in Transaction.Splits)
                    {
                        if (!_splitCategories[t].SelectMany(c => c.BudgetItems).Contains(t.Budget))
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

                // The date changed to a new month, reload the budget categories
                _parentCategories = BudgetDataService.GetBudgetCategories(((DateTime)_transactionDateBinder).Year, ((DateTime)_transactionDateBinder).Month, _currentUserId);
            }
        }

        /// <summary>
        /// Reloads the budget items for the newly selected month of the split
        /// </summary>
        /// <param name="split"></param>
        protected void SplitMonthChanged(Transaction split)
        {
            // Check for an existing entry
            if (_splitCategories.ContainsKey(split))
            {
                if (((DateTime)_splitDateBinders[split]).Year != split.TransactionDate.Year || ((DateTime)_splitDateBinders[split]).Month != split.TransactionDate.Month)
                {
                    // Clear the existing budget
                    split.Budget = null;

                    // Update the existing entry
                    _splitCategories[split] = BudgetDataService.GetBudgetCategories(((DateTime)_splitDateBinders[split]).Year, ((DateTime)_splitDateBinders[split]).Month, _currentUserId);
                }   
            }
            else
            {
                // No existing entry, add one
                _splitCategories.TryAdd(split, BudgetDataService.GetBudgetCategories(((DateTime)_splitDateBinders[split]).Year, ((DateTime)_splitDateBinders[split]).Month, _currentUserId));
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
            _splitCategories.TryAdd(transactionToAdd, BudgetDataService.GetBudgetCategories(transactionToAdd.TransactionDate.Year, transactionToAdd.TransactionDate.Month, _currentUserId));
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
            if (!IsSplitsAmountsCorrect(out decimal amountRemaining))
            {
                _remainingSplitsMessage = String.Format("Need {0:C} more", amountRemaining);
                _hideRemainingSplits = false;
                _isSplitsError = true;
            }
            else
            {
                _hideRemainingSplits = true;
                _isSplitsError = false;
            }
            //_isSplitsError = !IsSplitsAmountsCorrect();
        }

        /// <summary>
        /// Checks to see if the split amounts add to the total
        /// </summary>
        /// <returns></returns>
        private bool IsSplitsAmountsCorrect(out decimal amountRemaining)
        {
            bool ret = false;
            amountRemaining = 0;

            // Check that all of the split amounts add up to the total
            decimal totalSplits = 0;
            foreach (Transaction split in Transaction.Splits)
            {
                totalSplits += split.Amount;
            }

            amountRemaining = Transaction.Amount - totalSplits;

            if (Transaction.Splits.Count == 0 || totalSplits == Transaction.Amount)
            {
                ret = true;
            }

            return ret;
        }
    }
}
