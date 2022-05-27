using BudgetBlazor.DataAccess.Models;
using BudgetBlazor.DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Utilities;

namespace BudgetBlazor.Pages.Page_Components
{
    public class BudgetCategoryDisplayBase : ComponentBase
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

        [Parameter] public BudgetCategory Category { get; set; }
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public EventCallback<bool> IsLoadingChanged { get; set; }

        private BudgetItem itemBeforeEdit;

        /// <summary>
        /// Function to forcably hide the table loading indicator
        /// </summary>
        public void HideLoadingIndicator()
        {
            IsLoading = false;
            StateHasChanged();
        }

        /// <summary>
        /// Opens the dialog to edit a budget category
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["CategoryName"] = Category.Name, ["CategoryColor"] = new MudColor(Category.Color) };
            var dialogRef = DialogService.Show<EditCategoryDialog>("Edit Category", parameters);

            // Wait for a response and update the Category name and color
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                Tuple<string, string> data = (Tuple<string, string>)res.Data;
                Category.Name = data.Item1;
                Category.Color = data.Item2;
                BudgetDataService.Update(Category);
            }
        }

        /// <summary>
        /// Opens the dialog to add a budget to the category
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddBudgetDialog()
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
                Category.BudgetItems.Add(item);
                BudgetDataService.Update(Category);
            }
        }

        /// <summary>
        /// Calls the callback to delete the category
        /// </summary>
        protected async Task DeleteCategory()
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "Deleting a category will delete all budgets inside of it and cannot be undone!",
                yesText: "Delete!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Delete the category
                BudgetDataService.Delete(Category);
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

        /// <summary>
        /// Saves the edited budget item
        /// </summary>
        /// <param name="itemToSave"></param>
        protected void SaveBudgetItem(object itemToSave)
        {
            // Clear the backup item
            itemBeforeEdit = null;

            // Update the budget
            BudgetDataService.Update((BudgetItem)itemToSave);
        }

        /// <summary>
        /// Creates a local backup of the item, in case the user cancels the edit
        /// </summary>
        /// <param name="element"></param>
        protected void BackupItem(object element)
        {
            itemBeforeEdit = new BudgetItem(((BudgetItem)element).Name)
            {
                Budget = ((BudgetItem)element).Budget
            };
        }

        /// <summary>
        /// Resets the item to the backup, user cancelled the edit
        /// </summary>
        /// <param name="element"></param>
        protected void ResetItemToOriginalValues(object element)
        {
            // Update the current item to original values
            ((BudgetItem)element).Budget = itemBeforeEdit.Budget;
            ((BudgetItem)element).Name = itemBeforeEdit.Name;

            // Clear the backup item
            itemBeforeEdit = null;
        }
    }
}
