using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for ImportServerWindow.xaml
    /// </summary>
    public partial class ImportServerWindow : Window
    {
        private static readonly string _applicationDataPath = MainWindow.ApplicationDataPath;

        public ImportServerWindow()
        {
            InitializeComponent();
        }

        private void Browse_Button_Clicked(object sender, RoutedEventArgs e)
        {
            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = $@"Quick access";
            dialog.EnsurePathExists = true;
            dialog.IsFolderPicker = false;
            dialog.Title = "Select your server jar file";
            dialog.Filters.Add(new CommonFileDialogFilter("jar Files", "*.jar*"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                serverPathInput.Text = Path.GetFullPath(dialog.FileName);
            }
        }

        private void Cancel_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            win.DialogResult = false;
            win.Close();
        }

        private void Import_Button_Clicked(object sender, RoutedEventArgs e)
        {
            int ram = 0;
            var nameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_\s-.]*$");
            if (!nameRegex.IsMatch(serverNameInput.Text))
            {
                errorText.Text = "Invalid Server Name!";
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

            ImportServer();
        }

        private void ImportServer()
        {
            var ServerPath = Path.GetDirectoryName(serverPathInput.Text) + @"\";
            string ServerData = "#FissionMSL data file" +
                            "\nname=" + serverNameInput.Text +
                            "\njar-path=" + serverPathInput.Text +
                            "\nfile-path=" + ServerPath +
                            "\nram-allocation=" + serverRamInput.Text +
                            "\ndate=never";

            File.WriteAllText(ServerPath + serverNameInput.Text + ".fmsl", ServerData);

            List<string> pathLines = new List<string>();

            try
            {
                if (File.Exists(_applicationDataPath + "ServerList.txt"))
                {
                    pathLines = File.ReadAllLines(_applicationDataPath + "ServerList.txt").ToList();
                    pathLines.Add(ServerPath + serverNameInput.Text + ".fmsl");
                    File.WriteAllLines(_applicationDataPath + "ServerList.txt", pathLines);
                }
                else
                {
                    Directory.CreateDirectory(_applicationDataPath);
                    File.WriteAllText(_applicationDataPath + "ServerList.txt", ServerPath + serverNameInput.Text + ".fmsl");
                }
            }
            catch
            {

            }

            var win = Window.GetWindow(this);
            win.DialogResult = true;
            win.Close();
        }
    }
}
