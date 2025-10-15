// Made by Kieran Kelly
// Last changed on 2025-05-12 at 00:05
// 1980 was only 25 years ago... right?

using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Windows;
using System.Net;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for AppSetupDialog.xaml
    /// </summary>
    public partial class AppSetupDialog : Window
    {
        public AppSetupDialog()
        {
            InitializeComponent();
            Loaded += AppSetupDialog_Loaded;
        }

        private void AppSetupDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                progBar1.IsIndeterminate = false;
                WebClient webClient = new();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadJavaDoneCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("https://download.oracle.com/java/21/archive/jdk-21.0.6_windows-x64_bin.zip"), Directory.GetCurrentDirectory() + @"\jdk-21.0.6.zip");
            }
            catch
            {
                DialogResult = false;
                Close();
            }
        }

        // Progress changed callback.
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progBar1.Value = e.ProgressPercentage;
        }

        // Java download completed callback.
        private void DownloadJavaDoneCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                progBar1.IsIndeterminate = true;
                ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + @"\jdk-21.0.6.zip", Directory.GetCurrentDirectory(), true);
                File.Delete(Directory.GetCurrentDirectory() + @"\jdk-21.0.6.zip");

                DialogResult = true;
                Close();
            }
            catch
            {
                try
                {
                    DialogResult = false;
                }
                catch { }

                Close();
            }
        }
    }
}
