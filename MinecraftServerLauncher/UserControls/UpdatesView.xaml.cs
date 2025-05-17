// Made by Kieran Kelly
// Last changed on 2025-05-17 at 01:14
// First we mine, then we craft!

using System.Windows.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Net;
using System.Text.RegularExpressions;
using MinecraftServerLauncher.ViewModels;
using System.Windows.Documents;

namespace MinecraftServerLauncher.UserControls
{
    /// <summary>
    /// Handy version struct to help with version control.
    /// </summary>
    public struct Version
    {
        internal static Version zero = new Version(0, 0, 0, 0);

        private ushort release;
        private ushort major;
        private ushort minor;
        private ushort patch;

        internal Version(ushort _release, ushort _major, ushort _minor, ushort _patch)
        {
            release = _release;
            major = _major;
            minor = _minor;
            patch = _patch;
        }
        internal Version(string _version)
        {
            string[] versionStrings = _version.Split('.');
            if (versionStrings.Length != 4)
            {
                release = 0;
                major = 0;
                minor = 0;
                patch = 0;
                return;
            }

            release = ushort.Parse(versionStrings[0]);
            major = ushort.Parse(versionStrings[1]);
            minor = ushort.Parse(versionStrings[2]);
            patch = ushort.Parse(versionStrings[3]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (release != _otherVersion.release)
            {
                return true;
            }
            else
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (patch != _otherVersion.patch)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal bool IsLessThan(Version _otherVersion)
        {
            bool isTrue = false;

            if (patch < _otherVersion.patch)
            {
                isTrue = true;
            }
            if (patch > _otherVersion.patch)
            {
                isTrue = false;
            }

            if (minor < _otherVersion.minor)
            {
                isTrue = true;
            }
            if (minor > _otherVersion.minor)
            {
                isTrue = false;
            }

            if (major < _otherVersion.major)
            {
                isTrue = true;
            }
            if (major > _otherVersion.major)
            {
                isTrue = false;
            }

            if (release < _otherVersion.release)
            {
                isTrue = true;
            }
            if (release > _otherVersion.release)
            {
                isTrue = false;
            }

            return isTrue;
        }

        internal bool IsGreaterThan(Version _otherVersion)
        {
            bool isTrue = false;

            if (patch > _otherVersion.patch)
            {
                isTrue = true;
            }
            if (patch < _otherVersion.patch)
            {
                isTrue = false;
            }

            if (minor > _otherVersion.minor)
            {
                isTrue = true;
            }
            if (minor < _otherVersion.minor)
            {
                isTrue = false;
            }

            if (major > _otherVersion.major)
            {
                isTrue = true;
            }
            if (major < _otherVersion.major)
            {
                isTrue = false;
            }

            if (release > _otherVersion.release)
            {
                isTrue = true;
            }
            if (release < _otherVersion.release)
            {
                isTrue = false;
            }

            return isTrue;
        }

        public override string ToString()
        {
            return $"{release}.{major}.{minor}.{patch}";
        }
    }

    /// <summary>
    /// Interaction logic for UpdatesView.xaml
    /// </summary>
    public partial class UpdatesView : UserControl
    {
        private string rootPath;
        private string appZip;
        private Version onlineVersion;

        private bool isOutOfDate = false;
        private string onlineChangeLog;
        private string[] changeLogs;

        private List<ChangeLog> localChangeLogs = new();

        public UpdatesView()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            appZip = Path.Combine(rootPath, "Update.zip");

            CheckForUpdates();
            CheckForChangeLog();
        }

        private void Update_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.pendingUpdate != true)
            {
                topText.Text = "Downloading update...";
                updateButton.IsEnabled = false;
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("https://drive.google.com/uc?export=download&id=1gWYdfsnEikLNPf0PUea79CvwZOkvkaSw"), appZip);
            }
            else
            {
                MainWindow.Instance.Close();
            }
        }

        private void OnListBoxItem_Mouse_DoubleClick(object sender, RoutedEventArgs e)
        {
            ChangeLogViewerDialog changeLogViewer = new();
            changeLogViewer.Owner = MainWindow.Instance;
            changeLogViewer.changelogTitle.Text = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Name;
            changeLogViewer.changeList.Document.Blocks.Clear();
            changeLogViewer.changeList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Changes)));
            changeLogViewer.ShowDialog();
        }

        // Progress changed callback.
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        // Download completed callback.
        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            topText.Text = "Close to install update.";
            progressBar.Value = 100;
            MainWindow.Instance.pendingUpdate = true;
            updateButton.Content = "Restart";
            updateButton.IsEnabled = true;
        }

        // Checks online for any updates to the software.
        public async void CheckForUpdates()
        {
            topText.Text = "Checking for updates...";
            await Task.Run(() => CheckVersion());
        }

        // Checks online for the changelog.
        public async void CheckForChangeLog()
        {
            await Task.Run(() => DownloadChangeLog());
        }

        #region Callbacks

        private void CheckForUpdatesCallback()
        {
            if (isOutOfDate)
            {
                topText.Text = "New update available!";
                updateButton.IsEnabled = true;
            }
            else
            {
                topText.Text = "No updates found.";
            }
        }

        private void CheckForChangeLogCallback()
        {
            if (changeLogs != null)
            {
                for (int i = 0; i < changeLogs.Length; i++)
                {
                    string line = changeLogs[i];

                    if (line.StartsWith('>'))
                    {
                        ChangeLog changeLog = new();

                        changeLog.Name = line.Substring(line.IndexOf('>') + 1);

                        List<string> changeLines = new();
                        bool searchEnd = false;
                        int nextId = i + 1;

                        if (changeLogs[nextId].StartsWith('{'))
                        {
                            while (!searchEnd)
                            {
                                nextId++;
                                if (!changeLogs[nextId].StartsWith('}'))
                                {
                                    changeLines.Add(changeLogs[nextId]);
                                }
                                else
                                {
                                    searchEnd = true;
                                }
                            }

                            string content = "";

                            foreach (var change in changeLines)
                            {
                                content += change + "\n";
                            }

                            changeLog.Changes = content;

                            localChangeLogs.Add(changeLog);
                        }
                    }
                }

                if (localChangeLogs != null)
                {
                    loadingText.Visibility = Visibility.Collapsed;
                    localChangeLogs.Reverse();
                    LoadChangeLogs.AddChangeLogs(localChangeLogs);
                }
                else
                {
                    loadingText.Text = "Cannot load changelog!";
                }
            }
            else
            {
                loadingText.Text = "Cannot load changelog!";
            }
        }

        #endregion

        #region Threaded Functions

        // Downloads the latest version file from google drive and compares it to the local version.
        private void CheckVersion()
        {
            Version localVersion = MainWindow.InstalledVersion;

            Action acVc = () =>
            {
                versionText.Text = "Installed Version: " + localVersion.ToString();
            };
            Dispatcher.BeginInvoke(acVc);

            try
            {
                WebClient webClient = new WebClient();
                onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1qjmtXo6c48FkxR-71t8ljZ3BR3-gXl5g"));

                bool temp = false;

                if (onlineVersion.IsGreaterThan(localVersion))
                {
                    temp = true;
                }

                Action ac = () =>
                {
                    if (temp)
                        isOutOfDate = true;
                    latestVersionText.Text = "Latest Version: " + onlineVersion.ToString();
                    CheckForUpdatesCallback();
                };
                Dispatcher.BeginInvoke(ac);
            }
            catch
            {
                latestVersionText.Text = "Latest Version: Unknown";
            }
        }

        // Downloads the changelog from google drive.
        private void DownloadChangeLog()
        {
            string changeLogLink = "https://drive.google.com/uc?export=download&id=13JetsGXkoTH-4LXqQdSIELsdbFeZuGDS";

            try
            {
                WebClient webClient = new WebClient();
                onlineChangeLog = webClient.DownloadString(changeLogLink);

                Action ac = () =>
                {
                    changeLogs = Regex.Split(onlineChangeLog, "\r\n|\r|\n");
                    CheckForChangeLogCallback();
                };
                Dispatcher.BeginInvoke(ac);
            }
            catch
            {
                Action ac = () =>
                {
                    loadingText.Text = "Cannot load changelog!";
                };
                Dispatcher.BeginInvoke(ac);
            }
        }

        #endregion
    }
}
