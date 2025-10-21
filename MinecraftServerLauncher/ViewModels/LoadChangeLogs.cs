// Made by Kieran Kelly
// Last changed on 2025-10-21 at 02:08
// That's right, it goes in the square hole!

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Version = MinecraftServerLauncher.UserControls.Version;

namespace MinecraftServerLauncher.ViewModels
{

    public class ChangeLog
    {
        public string Name { get; set; }
        public string BugFixes { get; set; } = "None.";
        public string Additions { get; set; } = "None.";
        public string Changes { get; set; } = "None.";
        public Visibility ImportantUpdate { get; set; } = Visibility.Hidden;
        public string ImportantUpdateReason { get; set; } = string.Empty;
        public string UpdateType { get; set; }
        public Version VersionNum { get; set; }
        public GradientBrush RarityBorder { get; set; } = new LinearGradientBrush();
    }

    internal class LoadChangeLogs
    {
        private static readonly ObservableCollection<ChangeLog> _changeLogs = new ObservableCollection<ChangeLog>();
        /// <summary>
        /// List of servers in the server view.
        /// </summary>
        public static ReadOnlyObservableCollection<ChangeLog> ChangeLogs { get; }

        // Adds a changelog to the list of changelogs.
        public static void AddChangeLogs(List<ChangeLog> changeLogs)
        {
            foreach (var log in changeLogs)
            {
                _changeLogs.Add(log);
            }
        }

        // Constructor for LoadChangeLogs
        static LoadChangeLogs()
        {
            try
            {
                ChangeLogs = new ReadOnlyObservableCollection<ChangeLog>(_changeLogs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
