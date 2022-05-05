﻿using BudgetBlazor.Pages.Page_Components;
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
            BudgetDataService.AutomationDataChanged += BudgetDataService_AutomationDataChanged;

            //// DEBUG
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
            //// END DEBUG

            // Get the list of automations on load
            AutomationCategories = BudgetDataService.GetAutomationCategories(_currentUserId);
        }

        /// <summary>
        /// Opens the dialog to add a new budget category
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddCategoryDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["CategoryName"] = "", ["CategoryColor"] = null };
            var dialogRef = DialogService.Show<EditCategoryDialog>("Add New Category", parameters);

            // Wait for a response and add the Category
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Add the new category to the month
                Tuple<string, string> data = (Tuple<string, string>)res.Data;
                AutomationCategory category = new AutomationCategory(data.Item1, _currentUserId, data.Item2);
                category = BudgetDataService.CreateAutomationCategory(category);
                AutomationCategories.Add(category);
            }
        }

        #region Event Functions
        /// <summary>
        /// Subscriber to BudgetDataService changed event, updates UI when data has changed
        /// </summary>
        private void BudgetDataService_AutomationDataChanged()
        {
            // Model data has changed, reload the categories
            AutomationCategories = BudgetDataService.GetAutomationCategories(_currentUserId);
            StateHasChanged();
        }
        #endregion
    }
}
