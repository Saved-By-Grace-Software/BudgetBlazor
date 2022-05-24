using DataAccess.Models;
using DataAccess.Services;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterLib;

namespace BudgetBlazor.Helpers
{
    public static class ChartHelpers
    {
        public static Scatter GetScatterDataForAccount(int accountId, string scatterName, IBudgetDataService dataService)
        {
            Scatter scatter;

            // Get the account histories for the account
            List<AccountHistory> accountHistories = dataService.GetAccountHistory(accountId);

            // Iterate through the histories to create the required lists
            List<object> xData = new List<object>();
            List<object> yData = new List<object>();
            foreach (AccountHistory accountHistory in accountHistories)
            {
                xData.Add(accountHistory.BalanceDate);
                yData.Add(accountHistory.Balance);
            }

            scatter = new Scatter
            {
                Name = scatterName,
                Mode = ModeFlag.Lines | ModeFlag.Markers,
                X = xData,
                Y = yData
            };

            return scatter;
        }
    }
}
