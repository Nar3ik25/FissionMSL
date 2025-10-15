using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace UpdateHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string rootPath;
        string appZip;
        string appExe;

        public MainWindow()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            appZip = rootPath + @"\Update.zip";
            appExe = rootPath + @"\FissionMSL.exe";
            DownloadUpdate();
        }

        public void DownloadUpdate()
        {
            statText.Text = "Downloading...";
            progBar.IsIndeterminate = false;
            progBar.Value = 0;
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri("https://drive.google.com/uc?export=download&id=1gWYdfsnEikLNPf0PUea79CvwZOkvkaSw"), appZip);
        }

        // Progress changed callback.
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progBar.Value = e.ProgressPercentage;
        }

        // Download completed callback.
        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            SetupUpdate();
        }

        public void SetupUpdate()
        {
            statText.Text = "Installing...";
            progBar.IsIndeterminate = true;

            try
            {
                Debug.WriteLine(rootPath);
                ZipFile.ExtractToDirectory(appZip, rootPath, true);
                File.Delete(appZip);
                Process.Start(appExe, string.Empty);

                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                statText.Text = "Could not install update!";
                progBar.IsIndeterminate = false;
                progBar.Value = 100;
            }
        }
    }
}