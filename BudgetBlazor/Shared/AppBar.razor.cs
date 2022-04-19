using BudgetBlazor.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BudgetBlazor.Shared
{
	public partial class AppBar
	{
		private bool _isLightMode = true;
		private MudTheme _currentTheme = new MudTheme();

		[Parameter]
		public EventCallback OnSidebarToggled { get; set; }
		[Parameter]
		public EventCallback<MudTheme> OnThemeToggled { get; set; }

		private async Task ToggleTheme()
		{
			_isLightMode = !_isLightMode;

			_currentTheme = !_isLightMode ? GenerateDarkTheme() : new MudTheme();

			await OnThemeToggled.InvokeAsync(_currentTheme);
		}

		private MudTheme GenerateDarkTheme() =>
			new MudTheme
			{
				Palette = ColorHelpers.DarkPalette
			};
	}
}
