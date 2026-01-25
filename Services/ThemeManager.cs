using System;
using System.Windows;

namespace IsleServerLauncher.Services
{
    public class ThemeManager
    {
        /// <summary>
        /// Constructor accepts serverFolder for compatibility, but does not use it
        /// as persistence is now handled by ConfigurationManager.
        /// </summary>
        public ThemeManager(string serverFolder)
        {
            // No initialization needed.
        }

        /// <summary>
        /// Applies a theme to the application resources.
        /// </summary>
        /// <param name="themeName">"Light" or "Dark"</param>
        public void ApplyTheme(string themeName)
        {
            try
            {
                // Fallback for safety
                if (string.IsNullOrWhiteSpace(themeName))
                    themeName = "Light";

                var uri = new Uri($"Themes/{themeName}Theme.xaml", UriKind.Relative);

                // Load the theme dictionary
                ResourceDictionary? resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

                if (resourceDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
            }
            catch (Exception ex)
            {
                // Log via debug, avoiding UI crashes for cosmetic issues
                System.Diagnostics.Debug.WriteLine($"Error loading theme '{themeName}': {ex.Message}");
            }
        }
    }
}