// Made by Kieran Kelly
// Last changed on 2025-10-15 at 03:34
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
        public string MOTD = "";
        public string Seed = "";
        public string gamemode = "";
        public DateTime date = DateTime.MinValue;
        public bool hardcore = false;

        public ServerSettingsView()
        {
            InitializeComponent();
        }

        // Validates and saves the changes made.
        private void Save_Button_Clicked(object sender, RoutedEventArgs e)
        {
            int ram = 0;
            int port = 0;
            int renderDist = 0;

            ramErrorText.Text = "";
            IPPortErrorText.Text = "";
            renderDistErrorText.Text = "";

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
            else if (!Int32.TryParse(serverPortInput.Text, out port))
            {
                IPPortErrorText.Text = "Invalid Server Port!";
                return;
            }
            else if (port < 1025 || port > 65534)
            {
                IPPortErrorText.Text = "Invalid Server Port!";
                return;
            }
            else if (!Int32.TryParse(serverRenderDistInput.Text, out renderDist))
            {
                renderDistErrorText.Text = "Invalid Render Distance!";
                return;
            }
            else if (renderDist < 2)
            {
                renderDistErrorText.Text = "Render Distance cannot be lower than 2!";
                return;
            }
            else if (renderDist > 32)
            {
                renderDistErrorText.Text = "Render Distance cannot be higher than 32!";
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

                string sDate;
                if (date == DateTime.MinValue)
                {
                    sDate = "never";
                }
                else
                {
                    sDate = date.ToString();
                }

                string commands = "false";
                if (serverCommandBlockToggle.IsChecked == true)
                {
                    commands = "true";
                }

                string pvp = "false";
                if (serverPvPToggle.IsChecked == true)
                {
                    pvp = "true";
                }

                string ServerData = "#FissionMSL data file" +
                                    "\nname=" + serverName.Text +
                                    "\njar-path=" + ServerPath +
                                    "\nfile-path=" + ServerFilePath +
                                    "\nram-allocation=" + ram +
                                    "\ndate=" + sDate;

                using (var sw = File.CreateText(ServerFilePath + "server.properties"))
                {
                    sw.Write(string.Format(CreateServerWindow.PropertiesTemplate, serverIPInput.Text, serverPortInput.Text, serverRenderDistInput.Text, gamemode, hardcore, Seed, MOTD, pvp, commands));
                    sw.Close();
                }

                File.WriteAllText(ServerFilePath + serverName.Text + ".fmsl", ServerData);
            }
        }

        private void BacktoList_Button_Clicked(object sender, RoutedEventArgs e)
        {
            LoadServers.RefreshList();
            MainWindow.Instance.CloseServerSettings();
        }

        #region Settings Buttons

        // Copies the IP and Port to the user's clipboard.
        private void CopyAddress_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var ip = serverIPInput.Text;
            var port = serverPortInput.Text;
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
