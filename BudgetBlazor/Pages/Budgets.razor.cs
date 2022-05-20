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
        protected BudgetMonth _currentMonth = new BudgetMonth(DateTime.Now.Year, DateTime.Now.Month, new Guid());
        protected Guid _currentUserId;
        protected MudDatePicker _datePicker;
        private int _currentMonthId = -1;

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
        /// Lifecycle method called after the page is rendered
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || _currentMonth.Id != _currentMonthId)
            {
                // Update the month totals after the page is rendered the first time
                BudgetDataService.UpdateMonthTotals(_currentMonth);

                // Update the current month Id so we don't constantly recalculate
                _currentMonthId = _currentMonth.Id;

                Snackbar.Add("Loaded: " + ((DateTime)_currentMonthDate).ToString("MMMM yyyy"));
            }
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

        /// <summary>
        /// Resets the month to the default budgets
        /// </summary>
        protected async Task ResetMonthToDefault()
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "Resetting the month will delete all current budgets!",
                yesText: "Reset!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Reset the month
                _currentMonth = BudgetDataService.ResetMonthToDefault(_currentMonth, _currentUserId);
            }
        }

        /// <summary>
        /// Opens the date picker dialog to choose the month to view
        /// </summary>
        /// <returns></returns>
        protected async Task OpenDatePicker()
        {
            if (_datePicker != default(MudDatePicker))
            {
                _datePicker.Open();
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
                ChangeMonth(((DateTime)_currentMonthDate).AddMonths(1));
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
                ChangeMonth(((DateTime)_currentMonthDate).AddMonths(-1));
            }
        }

        /// <summary>
        /// Event to handle when the date picker chooses a new month
        /// </summary>
        protected void MonthChanged()
        {
            if (_currentMonthDate.HasValue)
            {
                // Trigger the change to the month's data
                ChangeMonth(_currentMonthDate);
            }
        }

        /// <summary>
        /// Reloads the data for the newly selected month
        /// </summary>
        /// <param name="newDate">The new month</param>
        protected void ChangeMonth(DateTime? newDate)
        {
            if (newDate.HasValue)
            {
                _currentMonthDate = newDate;
                _currentMonth = BudgetDataService.GetOrCreate(((DateTime)_currentMonthDate).Year, ((DateTime)_currentMonthDate).Month, _currentUserId);
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
