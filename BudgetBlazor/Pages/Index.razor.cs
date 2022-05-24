using BudgetBlazor.Helpers;
using DataAccess.Models;
using DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Plotly.Blazor;
using Title = Plotly.Blazor.LayoutLib.Title;
using YAxis = Plotly.Blazor.LayoutLib.YAxis;

namespace BudgetBlazor.Pages
{
    public class IndexBase : ComponentBase
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

        protected PlotlyChart chart;
        protected Config config { get; set; }
        protected Layout layout { get; set; }
        protected IList<ITrace> data { get; set; }
        protected List<Account> _userAccounts { get; set; }
        protected List<PiggyBank> _userBanks { get; set; }
        protected Guid _currentUserId;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            // Get the user accounts
            _userAccounts = BudgetDataService.GetAllAccounts(_currentUserId);

            // Iterate through the accounts and add their histories to the chart
            data = new List<ITrace>();
            foreach (Account account in _userAccounts)
            {
                data.Add(ChartHelpers.GetScatterDataForAccount(account.Id, account.Name, BudgetDataService));
            }

            // Configure the chart
            config = new()
            {
                Responsive = true,
                DisplayModeBar = Plotly.Blazor.ConfigLib.DisplayModeBarEnum.False
            };

            layout = new()
            {
                Title = new Title 
                { 
                    Text = "Your Accounts", 
                    Font =  new Plotly.Blazor.LayoutLib.TitleLib.Font()
                    {
                        Size = 30
                    }
                },
                YAxis = new List<YAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title { Text = "Account Balance" }
                    }
                }
            };

            // Get the user piggy banks
            _userBanks = BudgetDataService.GetAllPiggyBanks(_currentUserId);
        }
    }
}
