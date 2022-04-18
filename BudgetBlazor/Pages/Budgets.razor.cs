using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BudgetBlazor.Pages
{
    public class BudgetsBase : ComponentBase
    {
        [Inject]
        protected ISnackbar Snackbar { get; set; }

        protected DateTime? _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

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
                Snackbar.Add("Loading: " + ((DateTime)newDate).ToString("MMMM yyyy"));
            }
            
        }
    }
}
