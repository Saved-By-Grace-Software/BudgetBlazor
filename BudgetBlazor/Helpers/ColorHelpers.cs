using MudBlazor;

namespace BudgetBlazor.Helpers
{
    public static class ColorHelpers
    {
        /// <summary>
        /// Returns the color code for a standard progress bar
        /// 0 - 75% - Green
        /// 75 - 95% - Orange
        /// 95%+ - Red
        /// </summary>
        /// <param name="percentComplete"></param>
        /// <returns></returns>
        public static Color ProgressBarColor(int percentComplete)
        {
            if (percentComplete <= 75)
            {
                return Color.Success;
            }
            else if (percentComplete > 75 && percentComplete < 95)
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
        /// 0 - 75% - Red
        /// 75 - 95% - Orange
        /// 95%+ - Green
        /// </summary>
        /// <param name="percentComplete"></param>
        /// <returns></returns>
        public static Color ProgressBarColorReverse(int percentComplete)
        {
            if (percentComplete <= 75)
            {
                return Color.Error;
            }
            else if (percentComplete > 75 && percentComplete < 95)
            {
                return Color.Warning;
            }
            else
            {
                return Color.Success;
            }
        }
    }
}
