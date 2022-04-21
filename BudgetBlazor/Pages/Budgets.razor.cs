using BudgetBlazor.Pages.Page_Components;
using DataAccess.Services;
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

        // Parameters for the month being displayed
        protected DateTime? _currentMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        protected BudgetMonth _currentMonth;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            _currentMonth = BudgetMonthService.GetOrCreate(((DateTime)_currentMonthDate).Year, ((DateTime)_currentMonthDate).Month);
        }

        /// <summary>
        /// Opens the dialog to edit the expected income
        /// </summary>
        protected async Task OpenEditIncomeDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["ExpectedIncome"] = _currentMonth.ExpectedIncome };
            var dialogRef = DialogService.Show<EditIncomeDialog>("Edit Expected Income", parameters);

            // Wait for a response and update the expected income
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                _currentMonth.ExpectedIncome = (decimal)res.Data;
                BudgetMonthService.Update(_currentMonth);
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
                _currentMonth.BudgetCategories.Add(budgetCategory);
                BudgetMonthService.Update(_currentMonth);
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
            _currentMonth.BudgetCategories.Remove(categoryToDelete);

            // Update the month totals
            _currentMonth.UpdateMonthTotals();
        }

        #region Switch Month Functions
        /// <summary>
        /// Changes to the next month
        /// </summary>
        protected void NextMonth()
        {
            if (_currentMonthDate.HasValue)
            {
                // Trigger the change to the next month's data
                MonthChanged(((DateTime)_currentMonthDate).AddMonths(1));
            }
        }

        /// <summary>
        /// Changes to the previous month
        /// </summary>
        protected void LastMonth()
        {
            if (_currentMonthDate.HasValue)
            {
                // Trigger the change to the previous month's data
                MonthChanged(((DateTime)_currentMonthDate).AddMonths(-1));
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
                _currentMonthDate = newDate;
                _currentMonth = BudgetMonthService.GetOrCreate(((DateTime)_currentMonthDate).Year, ((DateTime)_currentMonthDate).Month);

                Snackbar.Add("Loaded: " + ((DateTime)_currentMonthDate).ToString("MMMM yyyy"));
            }
        }
        #endregion
    }
}
