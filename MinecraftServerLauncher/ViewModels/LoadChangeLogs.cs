// Made by Kieran Kelly
// Last changed on 2025-04-28 at 00:27
// That's right, it goes in the square hole!

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MinecraftServerLauncher.ViewModels
{

    public class ChangeLog
    {
        public string Name { get; set; }
        public string Changes { get; set; }
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
