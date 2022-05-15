using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterLib;

namespace BudgetBlazor.Pages
{
    public class IndexBase : ComponentBase
    {
        protected PlotlyChart chart;
        protected Config config = new Config();
        protected Layout layout = new Layout();

        // Using of the interface IList is important for the event callback!
        protected IList<ITrace> data = new List<ITrace>
        {
            new Scatter
            {
                Name = "ScatterTrace",
                Mode = ModeFlag.Lines | ModeFlag.Markers,
                X = new List<object>{1,2,3},
                Y = new List<object>{1,2,3}
            }
        };
    }
}
