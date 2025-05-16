// Made by Kieran Kelly
// Last changed on 2025-05-11 at 17:42
// According to all known laws of aviation,

using System.Windows.Controls;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Drawing;

namespace MinecraftServerLauncher.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private static readonly string _applicationDataPath = MainWindow.ApplicationDataPath;

        public SettingsView()
        {
            InitializeComponent();
            versionText.Text = "Application Version " + MainWindow.InstalledVersion.ToString();
        }

        private void ApplySettings_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            bool isJavaPathValid = false;
            bool isLibraryPathValid = false;
            string javaPath = appJavaPathInput.Text;
            string libraryPath = appServerPathInput.Text;

            if (string.IsNullOrEmpty(javaPath))
            {
                appJavaPathErrorText.Text = "You must provide a Java path.";
            }
            else if (javaPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                appJavaPathErrorText.Text = "Invalid characters in Java path!";
            }
            else if (!File.Exists(appJavaPathInput.Text))
            {
                appJavaPathErrorText.Text = $"Path does not lead to 'java.exe'!";
            }
            else if (Path.GetFileName(javaPath) != "java.exe")
            {
                appJavaPathErrorText.Text = $"Path does not lead to 'java.exe'!";
            }
            else
            {
                isJavaPathValid = true;
            }

            if (string.IsNullOrEmpty(libraryPath))
            {
                serverPathErrorText.Text = "You must provide a server library path.";
            }
            else if (libraryPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                serverPathErrorText.Text = "Invalid characters in server library path!";
            }
            else if (!Directory.Exists(libraryPath))
            {
                serverPathErrorText.Text = "Invalid path for server library!";
            }
            else
            {
                isLibraryPathValid = true;
            }

            if (isLibraryPathValid && isJavaPathValid)
            {
                serverPathErrorText.Text = "";
                appJavaPathErrorText.Text = "";

                string[] lines = {appServerPathInput.Text, appJavaPathInput.Text};

                File.WriteAllLines(_applicationDataPath + "AppSettings.txt", lines);
            }    
        }

        private void Browse_ServerLibraryPath_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = MainWindow.ApplicationServerPath;
            dialog.EnsurePathExists = true;
            dialog.IsFolderPicker = true;
            dialog.Title = "Select a folder.";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                appServerPathInput.Text = dialog.FileName;
            }
        }

        private void Default_ServerLibraryPath_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            appServerPathInput.Text = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\FissionMSL\";
        }

        private void Browse_JavaPath_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = Path.GetDirectoryName(MainWindow.ApplicationJavaPath);
            dialog.EnsurePathExists = true;
            dialog.IsFolderPicker = false;
            dialog.Title = "Select a Java runtime.";
            dialog.Filters.Add(new CommonFileDialogFilter("java.exe", "*.exe*"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                appJavaPathInput.Text = dialog.FileName;
            }
        }

        private void Default_JavaPath_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            appJavaPathInput.Text = Directory.GetCurrentDirectory() + @"\jdk-21.0.6\bin\java.exe";
        }
    }
}
