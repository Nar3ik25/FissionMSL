// Made by Kieran Kelly
// Last changed on 2025-05-17 at 00:44
// Don't you love when the errors solve themselves?

using MinecraftServerLauncher.ViewModels;
using Version = MinecraftServerLauncher.UserControls.Version;
using Color = System.Windows.Media.Color;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The application data path.
        public static readonly string ApplicationDataPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\FissionMSL\";

        // The application version number (Must be changed for each update!)
        public static readonly Version InstalledVersion = new Version(0, 3, 0, 0);

        // Java path for the application.
        public static string ApplicationJavaPath = null;

        // Server path for the application. (just where new servers are put, does not affect already made servers)
        public static string ApplicationServerPath = null;

        // Singleton instance of MainWindow
        public static MainWindow Instance { get; private set; }


        public bool pendingUpdate = false;

        private int attempts = 0;
        private bool closePressedWhileStopping = false;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Closing += OnMainWindowClosing;
            Loaded += OnMainWindowLoaded;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            CheckPaths();
        }

        // Verifies that all of the application paths are not null, and sets them if they are.
        public static void CheckPaths()
        {
            try
            {
                bool fileNotFound = true;

                if (File.Exists(ApplicationDataPath + "AppSettings.txt"))
                {
                    List<string> settingsLines = File.ReadAllLines(ApplicationDataPath + "AppSettings.txt").ToList();

                    if (settingsLines.Count == 2)
                    {
                        fileNotFound = false;

                        ApplicationServerPath = settingsLines[0];
                        ApplicationJavaPath = settingsLines[1];
                    }
                }

                if (fileNotFound)
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\jdk-21.0.6\bin\java.exe"))
                    {
                        ApplicationJavaPath = Directory.GetCurrentDirectory() + @"\jdk-21.0.6\bin\java.exe";
                    }
                    else
                    {
                        AppSetupDialog appSetupDialog = new AppSetupDialog();
                        appSetupDialog.Owner = Instance;
                        if (appSetupDialog.ShowDialog() == true)
                        {
                            ApplicationJavaPath = Directory.GetCurrentDirectory() + @"\jdk-21.0.6\bin\java.exe";
                        }
                    }

                    if (Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\FissionMSL\"))
                    {
                        ApplicationServerPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\FissionMSL\";
                    }
                    else
                    {
                        Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\FissionMSL\");
                        ApplicationServerPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\FissionMSL\";
                    }

                    string[] lines = { ApplicationServerPath, ApplicationJavaPath };

                    File.WriteAllLines(ApplicationDataPath + "AppSettings.txt", lines);
                }
            }
            catch
            {
                // TODO: Probably should have some kind of error message for this.
            }
        }

        // Logic done when the closing event is triggered.
        private async void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            // If there is a pending update check some stuff.
            if (pendingUpdate)
            {
                // If a console process is running safely shut it down before closing the window.
                if (consoleControl.IsProcessRunning)
                {
                    if (consoleInputBox.IsEnabled)
                    {
                        e.Cancel = true;

                        WindowClosingPopup wcp = new();
                        wcp.Owner = this;
                        wcp.Show();

                        consoleControl.WriteInput("stop\n", Color.FromRgb(100, 100, 100), true);
                        consoleInputBox.IsEnabled = false;

                        await Task.Run(() => WaitForServerStop());

                        Close();
                    }
                    else
                    {
                        e.Cancel = true;

                        WindowClosingPopup wcp = new();
                        wcp.Owner = this;
                        wcp.Show();

                        closePressedWhileStopping = true;

                        return;
                    }
                }
                // Start the update helper program to finalize the update.
                else
                {
                    try
                    {
                        Process.Start(Directory.GetCurrentDirectory() + "\\UpdateHelper.exe", string.Empty);
                    }
                    catch { }
                    finally
                    {
                        pendingUpdate = false;
                    }
                }
            }

            // If a console process is running safely shut it down before closing the window.
            if (consoleControl.IsProcessRunning)
            {
                if (consoleInputBox.IsEnabled)
                {
                    e.Cancel = true;

                    WindowClosingPopup wcp = new();
                    wcp.Owner = this;
                    wcp.Show();

                    consoleControl.WriteInput("stop\n", Color.FromRgb(100, 100, 100), true);
                    consoleInputBox.IsEnabled = false;

                    await Task.Run(() => WaitForServerStop());

                    Close();
                }
                else
                {
                    e.Cancel = true;

                    WindowClosingPopup wcp = new();
                    wcp.Owner = this;
                    wcp.Show();

                    closePressedWhileStopping = true;

                    return;
                }
            }
        }

        // Closes the server settings panel.
        public void CloseServerSettings()
        {
            serverView.Visibility = Visibility.Visible;
            serverSettingsView.Visibility = Visibility.Hidden;
        }

        // Executes the given command on the server.
        public async void InputCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            var firstFourChars = command.Length <= 4 ? command : command.Substring(0, 4);
            if (string.Equals(firstFourChars, "stop", StringComparison.OrdinalIgnoreCase))
            {
                consoleInputBox.Text = "";
                stopButton.IsEnabled = false;
                if (consoleControl.IsProcessRunning)
                    consoleControl.WriteInput("stop\n", Color.FromRgb(100, 100, 100), true);
                consoleInputBox.IsEnabled = false;

                await Task.Run(() => WaitForServerStop());

                MainPanel.Visibility = Visibility.Visible;
                ConsolePanel.Visibility = Visibility.Hidden;
            }
            else
            {
                consoleControl.WriteInput(command, Color.FromRgb(100, 100, 100), true);
            }
        }

        #region ListBoxItem Button Functions

        // Starts the server specified by the inputs.
        public async void StartServer(string serverPath, int serverRam, string serverName)
        {
            if (File.Exists(serverPath) && ApplicationJavaPath != null)
            {
                if (consoleControl.IsProcessRunning)
                    consoleControl.StopProcess();

                consoleControl.ShowDiagnostics = false;
                consoleControl.IsInputEnabled = false;
                consoleInputBox.IsEnabled = true;

                stopButton.IsEnabled = true;
                serverNameText.Text = serverName;

                await Task.Run(() => WaitForServerStart(serverPath, serverRam));

                MainPanel.Visibility = Visibility.Hidden;
                ConsolePanel.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorDialog errorDialog = new ErrorDialog();
                errorDialog.Owner = MainWindow.Instance;
                errorDialog.ShowDialog();

                LoadServers.RefreshList();
            }
        }

        // Opens the settings for the specified server.
        public void OpenServerSettings(ServerData serverData)
        {
            serverSettingsView.serverName.Text = serverData.ServerName;
            serverSettingsView.serverIPInput.Text = serverData.ServerIP;
            serverSettingsView.serverPortInput.Text = serverData.ServerPort;
            serverSettingsView.serverRamInput.Text = serverData.ServerRam;
            serverSettingsView.serverRenderDistInput.Text = serverData.ServerRenderDistance;
            serverSettingsView.ServerPath = serverData.ServerPath;
            serverSettingsView.ServerFilePath = serverData.ServerFilePath;
            serverSettingsView.Seed = serverData.ServerSeed;
            serverSettingsView.MOTD = serverData.MOTD;
            serverSettingsView.gamemode = serverData.ServerGamemode;
            serverSettingsView.hardcore = serverData.ServerHardcore;
            serverSettingsView.serverPath.Text = serverData.ServerFilePath;
            serverSettingsView.ramErrorText.Text = "";
            serverSettingsView.IPErrorText.Text = "";
            serverSettingsView.portErrorText.Text = "";
            serverSettingsView.renderDistErrorText.Text = "";
            serverView.Visibility = Visibility.Hidden;
            serverSettingsView.Visibility = Visibility.Visible;
        }

        #endregion

        #region Catagory Buttons

        private void Server_CatagoryButton_Clicked(object sender, RoutedEventArgs e)
        {
            ChangePanelVisibility(0);
        }

        private void Documents_CatagoryButton_Clicked(object sender, RoutedEventArgs e)
        {
            ChangePanelVisibility(1);
        }

        private void Updates_CatagoryButton_Clicked(object sender, RoutedEventArgs e)
        {
            ChangePanelVisibility(2);
        }

        private void Settings_CatagoryButton_Clicked(object sender, RoutedEventArgs e)
        {
            ChangePanelVisibility(3);
        }

        private void ChangePanelVisibility(int id)
        {
            switch (id)
            {
                case 0:
                    serverView.Visibility = Visibility.Visible;
                    serverSettingsView.Visibility = Visibility.Hidden;
                    docView.Visibility = Visibility.Hidden;
                    updatesView.Visibility = Visibility.Hidden;
                    settingsView.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    serverView.Visibility = Visibility.Hidden;
                    serverSettingsView.Visibility = Visibility.Hidden;
                    docView.Visibility = Visibility.Visible;
                    updatesView.Visibility = Visibility.Hidden;
                    settingsView.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    serverView.Visibility = Visibility.Hidden;
                    serverSettingsView.Visibility = Visibility.Hidden;
                    docView.Visibility = Visibility.Hidden;
                    updatesView.Visibility = Visibility.Visible;
                    settingsView.Visibility = Visibility.Hidden;
                    break;
                case 3:
                    serverView.Visibility = Visibility.Hidden;
                    serverSettingsView.Visibility = Visibility.Hidden;
                    docView.Visibility = Visibility.Hidden;
                    updatesView.Visibility = Visibility.Hidden;
                    settingsView.appServerPathInput.Text = ApplicationServerPath;
                    settingsView.appJavaPathInput.Text = ApplicationJavaPath;
                    settingsView.Visibility = Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region Console Panel Buttons

        private async void On_ServerStopButton_Clicked(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = false;
            if (consoleControl.IsProcessRunning)
                consoleControl.WriteInput("stop\n", Color.FromRgb(100, 100, 100), true);
            consoleInputBox.IsEnabled = false;

            await Task.Run(() => WaitForServerStop());

            MainPanel.Visibility = Visibility.Visible;
            ConsolePanel.Visibility = Visibility.Hidden;
        }

        private void On_CommandsButton_Clicked(object sender, RoutedEventArgs e)
        {
            CommandWindow commandWindow = new();
            commandWindow.Owner = this;
            commandWindow.Show();
        }

        private async void consoleInputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var firstFourChars = consoleInputBox.Text.Length <= 4 ? consoleInputBox.Text : consoleInputBox.Text.Substring(0, 4);
                if (string.Equals(firstFourChars, "stop", StringComparison.OrdinalIgnoreCase))
                {
                    consoleInputBox.Text = "";
                    stopButton.IsEnabled = false;
                    if (consoleControl.IsProcessRunning)
                        consoleControl.WriteInput("stop\n", Color.FromRgb(100, 100, 100), true);
                    consoleInputBox.IsEnabled = false;

                    await Task.Run(() => WaitForServerStop());

                    MainPanel.Visibility = Visibility.Visible;
                    ConsolePanel.Visibility = Visibility.Hidden;
                }
                else
                {
                    consoleControl.WriteInput(consoleInputBox.Text, Color.FromRgb(100, 100, 100), true);
                    consoleInputBox.Text = "";
                }
            }
        }

        #endregion

        #region Threaded Functions

        // Starts a server ;)
        private void WaitForServerStart(string serverPath, int serverRam)
        {
            Action ac1 = () =>
            {
                consoleControl.ClearOutput();
                consoleControl.StartProcess(ApplicationJavaPath, "-Xmx" + serverRam + "G -jar \"" + Path.GetFileName(serverPath) + "\" nogui", Path.GetDirectoryName(serverPath));
            };
            Dispatcher.BeginInvoke(ac1);
        }

        // Checks to see if there is a process running and closes the console window if there isn't.
        private void WaitForServerStop()
        {
            bool goAgain = true;
            bool isRunning = true;

            while (goAgain)
            {
                Action ac1 = () =>
                {
                    if (consoleControl.IsProcessRunning)
                    {
                        isRunning = true;
                    }
                    else
                    {
                        isRunning = false;
                    }
                };
                Dispatcher.BeginInvoke(ac1);

                if (isRunning)
                {
                    attempts++;
                    if (attempts > 100)
                    {
                        Action ac2 = () =>
                        {
                            if (consoleControl.IsProcessRunning)
                            {
                                consoleControl.StopProcess();
                            }
                        };
                        Dispatcher.BeginInvoke(ac2);
                        attempts = 0;
                        goAgain = false;
                    }
                    Thread.Sleep(100);
                }
                else
                {
                    goAgain = false;
                }
            }

            if (closePressedWhileStopping)
            {
                Action close = () =>
                {
                    Close();
                };
                Dispatcher.BeginInvoke(close);
            }
        }

        #endregion

        #region Static Functions

        /// <summary>
        /// Gets the user's IPv4 address.
        /// </summary>
        /// <returns>
        /// The first IPv4 address found on the user's system.
        /// </returns>
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        /// <summary>
        /// Sets the user's clipboard to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">String to set the clipboard to.</param>
        /// <returns>
        /// A boolean value indicating if the operation succeeded or not.
        /// </returns>
        public static bool SetClipboard(string value)
        {
            if (value == null)
                return false;

            try
            {
                Process clipboardExecutable = new();
                clipboardExecutable.StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true,
                    FileName = @"clip",
                };
                clipboardExecutable.Start();
                clipboardExecutable.StandardInput.Write(value);
                clipboardExecutable.StandardInput.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        #endregion
    }
}