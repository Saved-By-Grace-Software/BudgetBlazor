using BudgetBlazor.Data.Models;
using BudgetBlazor.Pages.Page_Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BudgetBlazor.Pages
{
    public class BudgetsBase : ComponentBase
    {
        #region Dependency Injection Parameters
        [Inject]
        protected ISnackbar Snackbar { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }
        #endregion

        // Date for the month that we are currently viewing
        protected DateTime? _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // DEUBG - This is temporary until I learn EFCore
        protected BudgetMonth month = new BudgetMonth(2022, 4);

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
                // DEBUG - For testing before real data!
                month = new BudgetMonth(((DateTime)newDate).Year, ((DateTime)newDate).Month);
                // END DEBUG

                Snackbar.Add("Loaded: " + ((DateTime)newDate).ToString("MMMM yyyy"));
            }
        }
        #endregion

        /// <summary>
        /// Opens the dialog to edit the expected income
        /// </summary>
        protected async Task OpenDialog()
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
    }
}
