using BudgetBlazor.Pages.Page_Components;
using BudgetBlazor.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BudgetBlazor.Pages
{
    public class BudgetsBase : ComponentBase
    {
        #region Dependency Injection & Cascading Parameters
        [Inject]
        protected ISnackbar Snackbar { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBudgetMonthService BudgetMonthService { get; set; }

        [CascadingParameter]
        protected MudTheme CurrentTheme { get; set; }
        #endregion

        // Date for the month that we are currently viewing
        protected DateTime? _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // DEUBG - This is temporary until I learn EFCore
        protected BudgetMonth month;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            month = BudgetMonthService.Get(((DateTime)_currentMonth).Year, ((DateTime)_currentMonth).Month);
        }

        /// <summary>
        /// Opens the dialog to edit the expected income
        /// </summary>
        protected async Task OpenEditIncomeDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["ExpectedIncome"] = month.ExpectedIncome };
            var dialogRef = DialogService.Show<EditIncomeDialog>("Edit Expected Income", parameters);

            // Wait for a response and update the expected income
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                month.ExpectedIncome = (decimal)res.Data;
            }
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
                month.BudgetCategories.Add(budgetCategory);
            }
        }

        /// <summary>
        /// Callback to delete the budget category
        /// </summary>
        /// <param name="categoryToDelete"></param>
        protected void DeleteCategory(BudgetCategory categoryToDelete)
        {
            // Delete the category budgets
            categoryToDelete.BudgetItems.Clear();

            // Delete the category
            month.BudgetCategories.Remove(categoryToDelete);

            // Update the month totals
            month.UpdateMonthTotals();
        }

        #region Switch Month Functions
        /// <summary>
        /// Changes to the next month
        /// </summary>
        protected void NextMonth()
        {
            if (_currentMonth.HasValue)
            {
                // Update current month to the next month
                _currentMonth = ((DateTime)_currentMonth).AddMonths(1);

                // Trigger the change to the next month's data
                MonthChanged(_currentMonth.Value);
            }
        }

        /// <summary>
        /// Changes to the previous month
        /// </summary>
        protected void LastMonth()
        {
            if (_currentMonth.HasValue)
            {
                // Update the current month to the previous month
                _currentMonth = ((DateTime)_currentMonth).AddMonths(-1);

                // Trigger the change to the previous month's data
                MonthChanged(_currentMonth.Value);
            }
        }

        /// <summary>
        /// Reloads the data for the newly selected month
        /// </summary>
        /// <param name="newDate">The new month</param>
        protected void MonthChanged(DateTime? newDate)
        {
            if (newDate.HasValue)
            {
                month = BudgetMonthService.Get(((DateTime)_currentMonth).Year, ((DateTime)_currentMonth).Month);

                Snackbar.Add("Loaded: " + ((DateTime)newDate).ToString("MMMM yyyy"));
            }
        }
        #endregion
    }
}
