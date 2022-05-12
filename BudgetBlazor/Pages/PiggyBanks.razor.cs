using BudgetBlazor.Pages.Page_Components;
using DataAccess.Models;
using DataAccess.Services;
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

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _accounts = BudgetDataService.GetAllAccounts(_currentUserId);
            _banks = BudgetDataService.GetAllPiggyBanks(_currentUserId);
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
            }
        }
    }
}
