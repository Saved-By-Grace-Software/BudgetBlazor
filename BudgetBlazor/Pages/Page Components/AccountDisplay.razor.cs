using DataAccess.Models;
using DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace BudgetBlazor.Pages.Page_Components
{
    public class AccountDisplayBase : ComponentBase
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

        [Parameter] public string AccountId { get; set; }
        protected Account? Account { get; set; }
        protected Guid _currentUserId;

        protected List<BreadcrumbItem> _items = new List<BreadcrumbItem>();

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            if (int.TryParse(AccountId, out int id))
            {
                Account = BudgetDataService.GetAccount(id, _currentUserId);
            }

            _items = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Accounts", href: "Accounts", icon: Icons.Material.Filled.CreditCard),
                new BreadcrumbItem(Account == null ? "Account" : Account.Name, href: null, disabled: true)
            };

            BudgetDataService.AccountDataChanged += BudgetDataService_AccountDataChanged;
        }

        /// <summary>
        /// Opens the dialog to add a new transaction
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddTransactionDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["Transaction"] = new Transaction("") };
            var dialogRef = DialogService.Show<EditTransactionDialog>("Add New Transaction", parameters);

            // Wait for a response and add the Account
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Add the new transaction
                Transaction transaction = (Transaction)res.Data;
                Account.Transactions.Add(transaction);
                BudgetDataService.UpdateAccount(Account);
            }
        }

        /// <summary>
        /// Opens the dialog to edit a transaction
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditTransactionDialog(Transaction transactionToEdit)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["Transaction"] = transactionToEdit };
            var dialogRef = DialogService.Show<EditTransactionDialog>("Edit Transaction", parameters);

            // Wait for a response and add the Account
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Update the transaction
                Transaction transaction = (Transaction)res.Data;
                int index = Account.Transactions.FindIndex(t => t.Id == transaction.Id);
                if (index != -1)
                {
                    Account.Transactions[index] = transaction;
                    BudgetDataService.UpdateAccount(Account);
                }
            }
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
