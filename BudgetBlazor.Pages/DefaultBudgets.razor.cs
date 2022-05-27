using BudgetBlazor.PageComponents;
using BudgetBlazor.DataAccess.Models;
using BudgetBlazor.DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Utilities;

namespace BudgetBlazor.Pages
{
    public class DefaultBudgetsBase : ComponentBase 
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

        // Parameters for the default month being displayed
        protected BudgetMonth _defaultMonth;
        protected Guid _currentUserId;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _defaultMonth = BudgetDataService.GetDefaultMonth(_currentUserId);
            BudgetDataService.BudgetDataChanged += BudgetDataService_BudgetDataChanged;
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
                BudgetCategory budgetCategory = new BudgetCategory(data.Item1, data.Item2);
                _defaultMonth.BudgetCategories.Add(budgetCategory);
                BudgetDataService.Update(_defaultMonth);
            }
        }

        /// <summary>
        /// Opens the dialog to edit a budget category
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditDialog(BudgetCategory categoryToEdit)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["CategoryName"] = categoryToEdit.Name, ["CategoryColor"] = new MudColor(categoryToEdit.Color) };
            var dialogRef = DialogService.Show<EditCategoryDialog>("Edit Category", parameters);

            // Wait for a response and update the Category name and color
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                Tuple<string, string> data = (Tuple<string, string>)res.Data;
                categoryToEdit.Name = data.Item1;
                categoryToEdit.Color = data.Item2;
                BudgetDataService.Update(categoryToEdit);
            }
        }

        /// <summary>
        /// Opens the dialog to add a budget to the category
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddBudgetDialog(BudgetCategory categoryToAddTo)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["BudgetName"] = "" };
            var dialogRef = DialogService.Show<EditBudgetItemDialog>("Add New Budget", parameters);

            // Wait for a response and add the new budget item
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Add the new budget item
                BudgetItem item = new BudgetItem((string)res.Data);
                categoryToAddTo.BudgetItems.Add(item);
                BudgetDataService.Update(categoryToAddTo);
            }
        }

        /// <summary>
        /// Opens the dialog to edit a budget in the category
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditBudgetDialog(BudgetItem itemToEdit, BudgetCategory parentCategory)
        {
            // Open the dialog
            var parameters = new DialogParameters { ["BudgetName"] = itemToEdit.Name };
            var dialogRef = DialogService.Show<EditBudgetItemDialog>("Edit Budget", parameters);

            // Wait for a response and edit the budget item
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Update the budget item and parent category
                itemToEdit.Name = (string)res.Data;
                BudgetDataService.Update(parentCategory);
            }
        }

        /// <summary>
        /// Deletes the category
        /// </summary>
        protected async Task DeleteCategory(BudgetCategory categoryToDelete)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "Deleting a category will delete all budgets inside of it and cannot be undone!",
                yesText: "Delete!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Delete the category
                BudgetDataService.Delete(categoryToDelete);
            }
        }

        /// <summary>
        /// Shows a confirmation dialog and then deletes the budget
        /// </summary>
        protected async Task DeleteBudgetItem(BudgetItem itemToDelete)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "Deleting a budget cannot be undone!",
                yesText: "Delete!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Delete the budget
                BudgetDataService.Delete(itemToDelete);
            }
        }

        #region Event Functions
        /// <summary>
        /// Subscriber to BudgetDataService changed event, updates UI when data has changed
        /// </summary>
        private void BudgetDataService_BudgetDataChanged()
        {
            // Model data has changed, trigger a UI update
            StateHasChanged();
        }
        #endregion
    }
}
