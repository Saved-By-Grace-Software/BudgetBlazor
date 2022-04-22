using BudgetBlazor.Pages.Page_Components;
using DataAccess.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Authorization;

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
        protected IBudgetDataService BudgetDataService { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [CascadingParameter]
        protected MudTheme CurrentTheme { get; set; }
        #endregion

        // Parameters for the month being displayed
        protected DateTime? _currentMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        protected BudgetMonth _currentMonth;
        protected Guid _currentUserId;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            _currentMonth = BudgetDataService.GetOrCreate(((DateTime)_currentMonthDate).Year, ((DateTime)_currentMonthDate).Month, _currentUserId);
            BudgetDataService.BudgetDataChanged += BudgetDataService_BudgetDataChanged;
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
                BudgetDataService.Update(_currentMonth);
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
                BudgetDataService.Update(_currentMonth);
            }
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
                _currentMonth = BudgetDataService.GetOrCreate(((DateTime)_currentMonthDate).Year, ((DateTime)_currentMonthDate).Month, _currentUserId);

                Snackbar.Add("Loaded: " + ((DateTime)_currentMonthDate).ToString("MMMM yyyy"));
            }
        }
        #endregion

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
