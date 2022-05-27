using BudgetBlazor.Pages.Page_Components;
using BudgetBlazor.DataAccess.Models;
using BudgetBlazor.DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace BudgetBlazor.Pages
{
    public class PiggyBanksBase : ComponentBase
    {
        #region Dependency Injection & Cascading Parameters
        [Inject]
        protected ISnackbar Snackbar { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBudgetDataService BudgetDataService { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [CascadingParameter]
        protected MudTheme CurrentTheme { get; set; }
        #endregion

        protected Guid _currentUserId { get; set; }
        protected List<Account> _accounts;
        protected List<PiggyBank> _banks;
        protected Dictionary<Account, PiggyBankAccountTotals> _accountTotals = new Dictionary<Account, PiggyBankAccountTotals>();

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _accounts = BudgetDataService.GetAllPiggyBankAccounts(_currentUserId);
            _banks = BudgetDataService.GetAllPiggyBanks(_currentUserId);
            UpdateAccountTotals();
        }

        /// <summary>
        /// Opens the dialog to add an amount to the current amount saved
        /// </summary>
        protected async Task OpenAddAmountDialog(PiggyBank bank)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["OkButtonText"] = "Add" };
            var dialogRef = DialogService.Show<EditPBAmountSavedDialog>("Add Amount Saved", parameters);

            // Wait for a response and add the amount to bank's current amount
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                bank.CurrentAmount += (decimal)res.Data;
                BudgetDataService.Update(bank);

                // Update account totals
                UpdateAccountTotals();
            }
        }

        /// <summary>
        /// Opens the dialog to remove an amount to the current amount saved
        /// </summary>
        protected async Task OpenRemoveAmountDialog(PiggyBank bank)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["OkButtonText"] = "Remove" };
            var dialogRef = DialogService.Show<EditPBAmountSavedDialog>("Remove Amount Saved", parameters);

            // Wait for a response and remove the amount from the bank's current amount
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                bank.CurrentAmount -= (decimal)res.Data;
                BudgetDataService.Update(bank);

                // Update account totals
                UpdateAccountTotals();
            }
        }

        /// <summary>
        /// Opens the dialog to add a new piggy bank
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddPiggyBankDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["PiggyBank"] = new PiggyBank("", _currentUserId) };
            var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialogRef = DialogService.Show<EditPiggyBankDialog>("Add New Piggy Bank", parameters, options);

            // Wait for a response and add the piggy bank
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                PiggyBank newBank = BudgetDataService.Create((PiggyBank)res.Data);
                if (newBank != default(PiggyBank))
                {
                    _banks.Add(newBank);

                    // Update account totals
                    UpdateAccountTotals();
                }
            }
        }

        /// <summary>
        /// Opens the dialog to edit a piggy bank
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditPiggyBankDialog(PiggyBank bankToEdit)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["PiggyBank"] = bankToEdit };
            var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialogRef = DialogService.Show<EditPiggyBankDialog>("Edit Piggy Bank", parameters, options);

            // Wait for a response and add the piggy bank
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                BudgetDataService.Update((PiggyBank)res.Data);

                // Update account totals
                UpdateAccountTotals();
            }
        }

        /// <summary>
        /// Deletes the given piggy bank
        /// </summary>
        /// <param name="bank"></param>
        /// <returns></returns>
        protected async Task DeletePiggyBank(PiggyBank bankToDelete)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "Deleting a piggy bank cannot be undone!",
                yesText: "Delete!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Delete the piggy bank
                _banks.Remove(bankToDelete);
                BudgetDataService.Delete(bankToDelete);

                // Update account totals
                UpdateAccountTotals();
            }
        }

        /// <summary>
        /// Updates the piggy bank account totals
        /// </summary>
        private void UpdateAccountTotals()
        {
            // Iterate through each account
            foreach (Account account in _accounts)
            {
                // Calculate the totals for the account
                PiggyBankAccountTotals totals = CalculateAccountTotals(account);

                // Check for an existing entry in the dictionary
                if (_accountTotals.ContainsKey(account))
                {
                    // Update the existing entry
                    _accountTotals[account] = totals;
                }
                else
                {
                    // Account not in the dictionary yet, add it
                    _accountTotals.Add(account, totals);
                }
            }
        }

        /// <summary>
        /// Calculates the piggy bank account totals for the given account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private PiggyBankAccountTotals CalculateAccountTotals(Account account)
        {
            PiggyBankAccountTotals totals = new PiggyBankAccountTotals();

            // Get a list of all piggy banks for this account
            List<PiggyBank> accountBanks = _banks.Where(b => b.SavingsAccount.Id == account.Id).ToList();

            // Iterate through the banks and add up totals
            foreach(PiggyBank bank in accountBanks)
            {
                totals.TargetAmount += bank.TargetAmount;
                totals.SavedSoFar += bank.CurrentAmount;
                totals.LeftToSave += bank.RemainingAmount;
            }

            return totals;
        }

        /// <summary>
        /// Class for holding piggy bank account totals
        /// </summary>
        protected class PiggyBankAccountTotals
        {
            public decimal TargetAmount { get; set; }

            public decimal SavedSoFar { get; set; }

            public decimal LeftToSave { get; set; }
        }
    }
}
