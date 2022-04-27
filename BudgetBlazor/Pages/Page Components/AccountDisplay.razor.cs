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
        }
    }
}
