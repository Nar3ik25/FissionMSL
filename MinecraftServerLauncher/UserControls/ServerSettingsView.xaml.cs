// Made by Kieran Kelly
// Last changed on 2025-04-27 at 02:58
// 1..2..3..4.. what comes after 4?

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftServerLauncher.ViewModels;

namespace MinecraftServerLauncher.UserControls
{
    /// <summary>
    /// Interaction logic for ServerSettingsView.xaml
    /// </summary>
    public partial class ServerSettingsView : UserControl
    {
        public string ServerPath = null;
        public string ServerFilePath = null;
        public string JavaPath = null;
        public string gamemode = "";
        public bool hardcore = false;

        public ServerSettingsView()
        {
            InitializeComponent();
        }

        // Validates and saves the changes made and returns the user to the server view panel.
        private void BacktoList_Button_Clicked(object sender, RoutedEventArgs e)
        {
            int ram = 0;

            if (!Int32.TryParse(serverRamInput.Text, out ram))
            {
                ramErrorText.Text = "Invalid RAM Allocation!";
                return;
            }
            else if (ram < 2)
            {
                ramErrorText.Text = "RAM Allocation cannot be lower than 2GB!";
                return;
            }
            else if (ram > 32)
            {
                ramErrorText.Text = "RAM Allocation cannot be higher than 32GB!";
                return;
            }
            else
            {
                string hardcoreStr = "";
                if (hardcore)
                {
                    hardcoreStr = "true";
                }
                else
                {
                    hardcoreStr = "false";
                }

                string ServerData = "#FissionMSL data file" +
                                    "\nname=" + serverName.Text +
                                    "\njar-path=" + ServerPath +
                                    "\nfile-path=" + ServerFilePath +
                                    "\njava-path=" + JavaPath +
                                    "\nram-allocation=" + ram +
                                    "\ntime=" + DateTime.UtcNow +
                                    "\nport=" + serverPort.Text +
                                    "\nipv4=" + serverIP.Text +
                                    "\ngamemode=" + gamemode +
                                    "\nhardcore=" + hardcoreStr;

                File.WriteAllText(ServerFilePath + "fissionMSL.data", ServerData);
                LoadServers.RefreshList();
                MainWindow.Instance.CloseServerSettings();
            }
        }

        #region Settings Buttons

        // Copies the IP and Port to the user's clipboard.
        private void CopyAddress_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var ip = serverIP.Text;
            var port = serverPort.Text;
            MainWindow.SetClipboard(ip + ":" + port);
        }

        // Opens the folder where the server is stored on the user's system.
        private void OpenFolder_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ServerFilePath);
        }

        // Deletes the server's world file so that the next time the server starts it regenerates the world.
        private void ResetWorld_Button_Clicked(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog cd = new ConfirmationDialog();
            cd.Owner = MainWindow.Instance;

            if (cd.ShowDialog() == true)
            {
                try
                {
                    if (Directory.Exists(ServerFilePath + "world"))
                    {
                        FileSystem.DeleteDirectory(ServerFilePath + "world", DeleteDirectoryOption.DeleteAllContents);
                    }
                }
                catch
                {

                }

                MainWindow.Instance.CloseServerSettings();
                LoadServers.RefreshList();
            }
        }

        // Deletes the server from the user's system.
        private void DeleteServer_Button_Clicked(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog cd = new ConfirmationDialog();
            cd.Owner = MainWindow.Instance;

            if (cd.ShowDialog() == true )
            {
                try
                {
                    FileSystem.DeleteDirectory(ServerFilePath, DeleteDirectoryOption.DeleteAllContents);
                }
                catch
                {

                }

                MainWindow.Instance.CloseServerSettings();
                LoadServers.RefreshList();
            }
        }

        #endregion
    }
}
