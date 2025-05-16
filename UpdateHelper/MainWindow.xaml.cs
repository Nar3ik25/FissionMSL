using System.Windows;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace UpdateHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupUpdate();
        }

        public void SetupUpdate()
        {
            string rootPath = Directory.GetCurrentDirectory();
            string appZip = rootPath + @"\Update.zip";
            string appExe = rootPath + @"\FissionMSL.exe";

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