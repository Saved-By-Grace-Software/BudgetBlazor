using DataAccess.Models;
using DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace BudgetBlazor.Pages
{
    public class AutomationsBase : ComponentBase
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

        protected List<AutomationCategory> AutomationCategories { get; set; }
        protected Guid _currentUserId { get; set; }

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            // DEBUG
            //BudgetMonth mon = BudgetDataService.GetDefaultMonth(_currentUserId);
            //for (int i = 0; i < 2; i++)
            //{
            //    AutomationCategory cat = new AutomationCategory("Category " + i, _currentUserId);

            //    for (int j = 0; j < 3; j++)
            //    {
            //        Automation aut = new Automation("Automation Rule " + j);
            //        aut.DefaultBudgetToSet = mon.BudgetCategories[0].BudgetItems[0];

            //        for (int k = 0; k < 2; k++)
            //        {
            //            AutomationRule rule = new AutomationRule("TEST " + k);
            //            aut.Rules.Add(rule);
            //        }

            //        cat.Automations.Add(aut);
            //    }

            //    BudgetDataService.CreateAutomationCategory(cat);
            //}
            // END DEBUG

            // Get the list of automations on load
            AutomationCategories = BudgetDataService.GetAutomationCategories(_currentUserId);
        }
    }
}
