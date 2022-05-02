﻿using DataAccess.Models;
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
            // Validate the form
            await form.Validate();

            if (form.IsValid)
            {
                // Update the transaction date from the binder
                Transaction.TransactionDate = (DateTime)_transactionDateBinder;

                // Clear the budget if the selected budget is not from the list
                if (!_parentBudgets.Contains(Transaction.Budget))
                {
                    Transaction.Budget = null;
                }

                // If income, clear the splits and the budget
                if (Transaction.IsIncome)
                {
                    Transaction.Budget = null;
                    Transaction.Splits.Clear();
                }

                // If split, remove budget from parent and update the splits
                if (Transaction.IsSplit)
                {
                    Transaction.Budget = null;

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
            // TODO: Fix this refresh to properly clear the cache and revert to the db transaction
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);

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
                // Update the existing entry
                _splitBudgets[split] = BudgetDataService.GetBudgetItems(((DateTime)_splitDateBinders[split]).Year, ((DateTime)_splitDateBinders[split]).Month, _currentUserId);
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
            // Check that all of the split amounts add up to the total
            decimal totalSplits = 0;
            foreach(Transaction split in Transaction.Splits)
            {
                totalSplits += split.Amount;
            }

            if (totalSplits != Transaction.Amount)
            {
                _isSplitsError = true;
            }
            else
            {
                _isSplitsError = false;
            }
        }

        /// <summary>
        /// Verifies that the split amounts add up to the total transaction amount
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected string VerifySplitAmounts(decimal arg)
        {
            if (arg == 0)
                return "Each split must have an amount specified";

            // Check that all of the split amounts add up to the total
            decimal totalSplits = 0;
            foreach (Transaction split in Transaction.Splits)
            {
                totalSplits += split.Amount;
            }

            if (totalSplits != Transaction.Amount)
            {
                return _splitsErrorMessage;
            }

            return null;
        }
    }
}
