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

            //// DEBUG
            //PiggyBank bank = new PiggyBank("Emergency Fund", _currentUserId)
            //{
            //    CurrentAmount = 1000,
            //    TargetAmount = 5000,
            //    TargetDate = new DateTime(2022, 12, 31)
            //};

            //List<Account> accounts = BudgetDataService.GetAllAccounts(_currentUserId);
            //bank.SavingsAccount = accounts[0];
            //BudgetDataService.Create(bank);

            //PiggyBank bank2 = new PiggyBank("Christmas", _currentUserId)
            //{
            //    CurrentAmount = 0,
            //    TargetAmount = 2000,
            //    TargetDate = new DateTime(2022, 11, 30)
            //};

            //bank2.SavingsAccount = accounts[0];
            //BudgetDataService.Create(bank2);
            //// END DEBUG
        }

        /// <summary>
        /// Opens the dialog to add an amount to the current amount saved
        /// </summary>
        protected async Task OpenAddAmountDialog(PiggyBank bank)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["OkButtonText"] = "Add" };
            var dialogRef = DialogService.Show<EditPBAmountSaved>("Add Amount Saved", parameters);

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
            var dialogRef = DialogService.Show<EditPBAmountSaved>("Remove Amount Saved", parameters);

            // Wait for a response and remove the amount from the bank's current amount
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                bank.CurrentAmount -= (decimal)res.Data;
                BudgetDataService.Update(bank);
            }
        }
    }
}
