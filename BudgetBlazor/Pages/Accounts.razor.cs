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

            // DEBUG
            _accounts = new List<Account>()
            {
                new Account("Account 1") { AccountType = AccountType.Checking, CurrentBalance = 500, LastUpdated = DateTime.Now},
                new Account("Account 2") { AccountType = AccountType.Savings, CurrentBalance = 500, LastUpdated = DateTime.Now},
                new Account("Account 3") { AccountType = AccountType.CreditCard, CurrentBalance = 500, LastUpdated = DateTime.Now},
                new Account("Account 4") { AccountType = AccountType.CreditCard, CurrentBalance = 500, LastUpdated = DateTime.Now}
            };
            // END DEBUG
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
                //// Add the new category to the month
                //Tuple<string, string> data = (Tuple<string, string>)res.Data;
                //BudgetCategory budgetCategory = new BudgetCategory(data.Item1, data.Item2);
                //_currentMonth.BudgetCategories.Add(budgetCategory);
                //BudgetDataService.Update(_currentMonth);
            }
        }
    }
}
