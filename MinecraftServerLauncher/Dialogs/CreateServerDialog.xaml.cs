// Made by Kieran Kelly
// Last changed on 2025-10-15 at 03:19
// Twenty-eight stab wounds!

using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for CreateServerWindow.xaml
    /// </summary>
    public partial class CreateServerWindow : Window
    {
        private string _serversPath = MainWindow.ApplicationServerPath;
        private static readonly string _applicationDataPath = MainWindow.ApplicationDataPath;
        string ServerFilePath = string.Empty;
        string ServerData = string.Empty;
        string JavaPath = string.Empty;
        string gamemode = "survival";
        string hardcore = "false";
        string MOTD = @"\u00a76\u2550\u2550 \u2605\u00a7b A Cool Minecraft Server \u00a76\u2605 \u2550\u2550\u00a7r\n\u00a77Hosted with\u00a7f Fission\u00a74 MSL";

        bool isImagePathFull = false;

        public CreateServerWindow()
        {
            InitializeComponent();

            serverIPInput.Text = MainWindow.GetLocalIPAddress();
        }

        private void BrowseImage_Button_Clicked(object sender, RoutedEventArgs e)
        {
            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = $@"Quick access";
            dialog.EnsurePathExists = true;
            dialog.IsFolderPicker = false;
            dialog.Title = "Select a .png image";
            dialog.Filters.Add(new CommonFileDialogFilter("PNG Files", "*.png*"));
            
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Bitmap img = new Bitmap(dialog.FileName);

                if (img.Width == 64 && img.Height == 64)
                {
                    serverIconPath.Text = Path.GetFullPath(dialog.FileName);
                    serverIconDisplay.Source = MainWindow.Bitmap2BitmapImage(img);
                }
                else
                    errorText.Text = "Selected server icon is not valid!";
            }
        }

        // Closes the dialog without creating a server.
        private void Cancel_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            win.DialogResult = false;
            win.Close();
        }

        // Validates selections and starts the create server funtion.
        private void CreateServer_Button_Clicked(object sender, RoutedEventArgs e)
        {
            ServerFilePath = _serversPath + serverNameInput.Text + @"\";
            int port = 0;
            int renderDist = 0;
            int ram = 0;
            var nameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_\s-.]*$");
            if (!nameRegex.IsMatch(serverNameInput.Text))
            {
                errorText.Text = "Invalid Server Name!";
                return;
            }
            else if (!Int32.TryParse(serverPortInput.Text, out port))
            {
                errorText.Text = "Invalid Server Port!";
                return;
            }
            else if (port < 1025 || port > 65534)
            {
                errorText.Text = "Invalid Server Port!";
                return;
            }
            else if (!Int32.TryParse(serverRenderDistInput.Text, out renderDist))
            {
                errorText.Text = "Invalid Render Distance!";
                return;
            }
            else if (renderDist < 2)
            {
                errorText.Text = "Render Distance cannot be lower than 2!";
                return;
            }
            else if (renderDist > 32)
            {
                errorText.Text = "Render Distance cannot be higher than 32!";
                return;
            }
            else if (!Int32.TryParse(serverRamInput.Text, out ram))
            {
                errorText.Text = "Invalid RAM Allocation!";
                return;
            }
            else if (ram < 2)
            {
                errorText.Text = "RAM Allocation cannot be lower than 2GB!";
                return;
            }
            else if (ram > 32)
            {
                errorText.Text = "RAM Allocation cannot be higher than 32GB!";
                return;
            }
            else if (Directory.Exists(ServerFilePath))
            {
                errorText.Text = "There is already a server with that name!";
                return;
            }
            else if (versionListBox.SelectedIndex == -1)
            {
                errorText.Text = "You need to select a version!";
                return;
            }
            else
            {
                errorText.Text = "";
            }

            if (!string.IsNullOrEmpty(serverIconPath.Text))
            {
                if (serverIconPath.Text != "Default icon")
                {
                    isImagePathFull = true;
                }
            }

            serverNameInput.IsEnabled = false;
            serverSeedInput.IsEnabled = false;
            versionListBox.IsEnabled = false;
            gamemodeButtons.IsEnabled = false;
            serverIPInput.IsEnabled = false;
            serverPortInput.IsEnabled = false;
            serverRenderDistInput.IsEnabled = false;
            serverRamInput.IsEnabled = false;
            serverIconPath.IsEnabled = false;
            browseIcons.IsEnabled = false;
            progressBar.IsIndeterminate = true;
            createButton.IsEnabled = false;
            cancelButton.IsEnabled = false;
            createButton.Content = "Creating...";

            CreateServer();
        }

        // Checks what version is currently selected.
        private void CheckVersionSelection(object sender, SelectionChangedEventArgs e)
        {
            if (versionListBox.SelectedIndex == versionListBox.Items.Count - 1)
            {
                if ((bool)hardcoreRadioButton.IsChecked)
                {
                    hardcoreRadioButton.IsChecked = false;
                    survivalRadioButton.IsChecked = true;
                    hardcore = "false";
                }

                hardcoreRadioButton.IsEnabled = false;
            }
            else
            {
                if (hardcoreRadioButton != null)
                {
                    hardcoreRadioButton.IsEnabled = true;
                }
            }
        }

        private void SeedTextBox_Preview_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(serverSeedInput.Text + e.Text);
        }

        // Regex that matches disallowed text.
        private static readonly Regex _regex = new Regex(@"^-?\d*$");
        private static bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }

        #region Gamemode Selection

        private void SetGamemodeSurvival(object sender, RoutedEventArgs e)
        {
            gamemode = "survival";
            hardcore = "false";
        }

        private void SetGamemodeCreative(object sender, RoutedEventArgs e)
        {
            gamemode = "creative";
            hardcore = "false";
        }

        private void SetGamemodeHardcore(object sender, RoutedEventArgs e)
        {
            gamemode = "survival";
            hardcore = "true";
        }

        #endregion

        #region Server Download and Creation

        // Downloads and sets up the server based off of the user's selections.
        public void CreateServer()
        {
            try
            {
                progressBar.IsIndeterminate = false;
                WebClient webClient = new();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                Directory.CreateDirectory(ServerFilePath);
                switch (versionListBox.SelectedIndex)
                {
                    case 0: // 1.21.10
                        File.WriteAllText(ServerFilePath + "1.21.10.version", "1.21.10");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/95495a7f485eedd84ce928cef5e223b757d2f764/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 1: // 1.21.9
                        File.WriteAllText(ServerFilePath + "1.21.9.version", "1.21.9");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/11e54c2081420a4d49db3007e66c80a22579ff2a/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 2: // 1.21.8
                        File.WriteAllText(ServerFilePath + "1.21.8.version", "1.21.8");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/6bce4ef400e4efaa63a13d5e6f6b500be969ef81/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 3: // 1.21.5
                        File.WriteAllText(ServerFilePath + "1.21.5.version", "1.21.5");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/e6ec2f64e6080b9b5d9b471b291c33cc7f509733/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 4: // 1.19.4
                        File.WriteAllText(ServerFilePath + "1.19.4.version", "1.19.4");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/8f3112a1049751cc472ec13e397eade5336ca7ae/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 5: // 1.16.5
                        File.WriteAllText(ServerFilePath + "1.16.5.version", "1.16.5");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/1b557e7b033b583cd9f66746b7a9ab1ec1673ced/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 6: // 1.12.2
                        File.WriteAllText(ServerFilePath + "1.12.2.version", "1.12.2");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/886945bfb2b978778c3a0288fd7fab09d315b25f/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 7: // 1.8.9
                        File.WriteAllText(ServerFilePath + "1.8.9.version", "1.8.9");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/b58b2ceb36e01bcd8dbf49c8fb66c55a9f0676cd/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 8: // 1.7.10
                        File.WriteAllText(ServerFilePath + "1.7.10.version", "1.7.10");
                        webClient.DownloadFileAsync(new Uri("https://piston-data.mojang.com/v1/objects/952438ac4e01b4d115c5fc38f891710c4941df29/server.jar"), ServerFilePath + "server.jar");
                        break;
                    case 9: // 1.0.0
                        File.WriteAllText(ServerFilePath + "1.0.0.version", "1.0.0");
                        webClient.DownloadFileAsync(new Uri("https://files.betacraft.uk/server-archive/release/1.0/1.0.0.jar"), ServerFilePath + "server.jar");
                        break;
                    default:
                        errorText.Text = "Version not selected!";
                        return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: Failed to install files!\nDetails: " + ex.Message);
            }
        }

        // Progress changed callback.
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        // Download completed callback.
        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            progressBar.IsIndeterminate = true;

            ServerData = "#FissionMSL data file" +
                            "\nname=" + serverNameInput.Text +
                            "\njar-path=" + ServerFilePath + "server.jar" +
                            "\nfile-path=" + ServerFilePath +
                            "\nram-allocation=" + serverRamInput.Text +
                            "\ndate=never";

            File.WriteAllText(ServerFilePath + "eula.txt", "eula=true");
            using (var sw = File.CreateText(ServerFilePath + "server.properties"))
            {
                sw.Write(string.Format(PropertiesTemplate, serverIPInput.Text, serverPortInput.Text, serverRenderDistInput.Text, gamemode, hardcore, serverSeedInput.Text, MOTD, "true", "true"));
                sw.Close();
            }
            File.WriteAllText(ServerFilePath + serverNameInput.Text + ".fmsl", ServerData);

            if (isImagePathFull)
            {
                try
                {
                    File.Copy(serverIconPath.Text, ServerFilePath + "server-icon.png");
                }
                catch {}
            }

            List<string> pathLines = new List<string>();

            try
            {
                if (File.Exists(_applicationDataPath + "ServerList.txt"))
                {
                    pathLines = File.ReadAllLines(_applicationDataPath + "ServerList.txt").ToList();
                    pathLines.Add(ServerFilePath + serverNameInput.Text + ".fmsl");
                    File.WriteAllLines(_applicationDataPath + "ServerList.txt", pathLines);
                }
                else
                {
                    Directory.CreateDirectory(_applicationDataPath);
                    File.WriteAllText(_applicationDataPath + "ServerList.txt", ServerFilePath + serverNameInput.Text + ".fmsl");
                }
            }
            catch
            {

            }

            var win = Window.GetWindow(this);
            win.DialogResult = true;
            win.Close();
        }

        #endregion

        // Server properties template
        public static readonly string PropertiesTemplate = @"#Minecraft server properties
#Tue Apr 22 21:36:34 PDT 2025
accepts-transfers=false
allow-flight=false
allow-nether=true
broadcast-console-to-ops=true
broadcast-rcon-to-ops=true
bug-report-link=
difficulty=normal
enable-command-block={8}
enable-jmx-monitoring=false
enable-query=false
enable-rcon=false
enable-status=true
enforce-secure-profile=true
enforce-whitelist=false
entity-broadcast-range-percentage=100
force-gamemode=false
function-permission-level=2
gamemode={3}
generate-structures=true
generator-settings={{}}
hardcore={4}
hide-online-players=false
initial-disabled-packs=
initial-enabled-packs=vanilla
level-name=world
level-seed={5}
level-type=minecraft\:normal
log-ips=true
max-chained-neighbor-updates=1000000
max-players=20
max-tick-time=60000
max-world-size=29999984
motd={6}
network-compression-threshold=256
online-mode=true
op-permission-level=4
pause-when-empty-seconds=60
player-idle-timeout=0
prevent-proxy-connections=false
pvp={7}
query.port=25565
rate-limit=0
rcon.password=
rcon.port=25575
region-file-compression=deflate
require-resource-pack=false
resource-pack=
resource-pack-id=
resource-pack-prompt=
resource-pack-sha1=
server-ip={0}
server-port={1}
simulation-distance={2}
spawn-animals=true
spawn-monsters=true
spawn-protection=0
sync-chunk-writes=true
text-filtering-config=
text-filtering-version=0
use-native-transport=true
view-distance={2}
white-list=false
";
    }
}
