using MudBlazor;

namespace BudgetBlazor.Helpers
{
    public static class ColorHelpers
    {
        /// <summary>
        /// Returns the color code for a standard progress bar
        /// 0 - {ok}% - Green
        /// {ok} - {bad}% - Orange
        /// {bad}%+ - Red
        /// </summary>
        /// <param name="percentComplete"></param>
        /// <returns></returns>
        public static Color ProgressBarColor(int percentComplete, int okThreshold = 75, int badThreshold = 95)
        {
            if (percentComplete <= okThreshold)
            {
                return Color.Success;
            }
            else if (percentComplete > okThreshold && percentComplete < badThreshold)
            {
                return Color.Warning;
            }
            else
            {
                return Color.Error;
            }
        }

        /// <summary>
        /// Returns the color code for a reversed progress bar
        /// 0 - {ok}% - Red
        /// {ok} - {good}% - Orange
        /// {good}%+ - Green
        /// </summary>
        /// <param name="percentComplete"></param>
        /// <returns></returns>
        public static Color ProgressBarColorReverse(int percentComplete, bool badOver100 = true, int okThreshold = 75, int goodThreshold = 95)
        {
            if (percentComplete <= okThreshold || (badOver100 && percentComplete > 100))
            {
                return Color.Error;
            }
            else if (percentComplete > okThreshold && percentComplete < goodThreshold)
            {
                return Color.Warning;
            }
            else
            {
                return Color.Success;
            }
        }

        /// <summary>
        /// Color palette definition for the dark theme
        /// </summary>
        public static Palette DarkPalette = new Palette()
        {
            Black = "#27272f",
			Background = "#32333d",
			BackgroundGrey = "#27272f",
			Surface = "#373740",
			TextPrimary = "#ffffffb3",
			TextSecondary = "rgba(255,255,255, 0.50)",
			AppbarBackground = "#27272f",
			AppbarText = "#ffffffb3",
			DrawerBackground = "#27272f",
			DrawerText = "#ffffffb3",
			DrawerIcon = "#ffffffb3"
        };

        // NEED TO ADD COLORS TO ABOVE PALETTE ^^^
        // GrayLight
        // GrayLighter
        // Tertiary
        // TertiaryContrastText
    }
}
