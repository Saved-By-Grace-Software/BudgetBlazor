using BudgetBlazor.Pages.Page_Components;
using DataAccess.Models;
using DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace BudgetBlazor.Pages
{
    public class AccountsBase : ComponentBase
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

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        protected MudTheme CurrentTheme { get; set; }
        #endregion

        protected Guid _currentUserId;
        protected List<Account> _accounts;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _accounts = BudgetDataService.GetAllAccounts(_currentUserId);
            BudgetDataService.AccountDataChanged += BudgetDataService_AccountDataChanged;
        }

        /// <summary>
        /// Opens the dialog to add a new account
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddAccountDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["Account"] = new Account("") };
            var dialogRef = DialogService.Show<EditAccountDialog>("Add New Account", parameters);

            // Wait for a response and add the Account
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Add the new account
                Account account = (Account)res.Data;
                _accounts = BudgetDataService.CreateAccount(account, _currentUserId);
            }
        }

        /// <summary>
        /// Opens the dialog to edit an account
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditAccountDialog(Account accountToEdit)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["Account"] = accountToEdit };
            var dialogRef = DialogService.Show<EditAccountDialog>("Add New Account", parameters);

            // Wait for a response and add the Account
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Add the new account
                Account account = (Account)res.Data;
                BudgetDataService.UpdateAccount(account);
            }
        }

        /// <summary>
        /// Shows a confirmation dialog and then deletes the account
        /// </summary>
        protected async Task DeleteAccount(Account accountToDelete)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "This will delete all of the account's transactions. Deleting an account cannot be undone!",
                yesText: "Delete!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Delete the budget
                BudgetDataService.DeleteAccount(accountToDelete);
            }
        }

        /// <summary>
        /// Navigates to the page for the specified account
        /// </summary>
        /// <param name="tableRowClickEventArgs"></param>
        protected void RowClickEvent(TableRowClickEventArgs<Account> tableRowClickEventArgs)
        {
            NavigationManager.NavigateTo("/Accounts/Edit/" + tableRowClickEventArgs.Item.Id);
        }

        #region Event Functions
        /// <summary>
        /// Subscriber to BudgetDataService account changed event, updates UI when account data has changed
        /// </summary>
        private void BudgetDataService_AccountDataChanged()
        {
            StateHasChanged();
        }
        #endregion
    }
}
