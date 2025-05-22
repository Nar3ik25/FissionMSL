// Made by Kieran Kelly
// Last changed on 2025-05-22 at 13:13
// Detecting multiple leviathan class lifeforms in the region. Are you certain whatever you're doing is worth it?

using System.Windows;
using System.Windows.Controls;
using MinecraftServerLauncher.ViewModels;

namespace MinecraftServerLauncher.UserControls
{

    /// <summary>
    /// Interaction logic for ServerView.xaml
    /// </summary>
    public partial class ServerView : UserControl
    {
        private bool isLoaded = false;

        public ServerView()
        {
            RefreshList();
            InitializeComponent();
            Loaded += OnMainWindowLoaded;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;

            if (LoadServers.Servers.Count == 0)
            {
                noServerText.Visibility = Visibility.Visible;
            }
            else
            {
                noServerText.Visibility = Visibility.Hidden;
            }
        }

        // Refreshes the list of servers.
        public void RefreshList()
        {
            LoadServers.RefreshList();

            if (LoadServers.Servers.Count == 0 && isLoaded)
            {
                noServerText.Visibility = Visibility.Visible;
            }
            else if (isLoaded)
            {
                noServerText.Visibility = Visibility.Hidden;
            }
        }

        // Launches the server that this button belongs to.
        private void Server_LaunchButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ListBoxItem listBoxItem = button.DataContext as ListBoxItem;

            foreach (var item in serversListBox.Items)
            {
                if (item == listBoxItem.Content)
                {
                    serversListBox.SelectedItem = item;
                }
            }

            int ram = Int32.Parse(LoadServers.Servers[serversListBox.SelectedIndex].ServerRam);
            string name = LoadServers.Servers[serversListBox.SelectedIndex].ServerName;
            MainWindow.Instance.StartServer(LoadServers.Servers[serversListBox.SelectedIndex]);
        }

        // Opens the settings panel for the server this button belongs to.
        private void Server_SettingsButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ListBoxItem listBoxItem = button.DataContext as ListBoxItem;

            foreach (var item in serversListBox.Items)
            {
                if (item == listBoxItem.Content)
                {
                    serversListBox.SelectedItem = item;
                }
            }

            MainWindow.Instance.OpenServerSettings(LoadServers.Servers[serversListBox.SelectedIndex]);
        }

        // Manually refreshes the server list.
        private void RefreshList_Button_Clicked(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        // Opens the server creator dialog.
        private void AddServer_Button_Clicked(object sender, RoutedEventArgs e)
        {
            CreateServerWindow createServerWindow = new CreateServerWindow();
            createServerWindow.Owner = MainWindow.Instance;
            if (createServerWindow.ShowDialog() == true)
            {
            }
            RefreshList();
        }
    }
}
