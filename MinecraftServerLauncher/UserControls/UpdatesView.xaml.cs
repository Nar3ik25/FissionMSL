// Made by Kieran Kelly
// Last changed on 2025-10-21 at 02:55
// First we mine, then we craft!

using MinecraftServerLauncher.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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

        internal string GetUpdateType(Version _previousVersion)
        {
            if (release > _previousVersion.release)
            {
                return "Release";
            }
            else
            {
                if (major > _previousVersion.major)
                {
                    return "Major Update";
                }
                else
                {
                    if (minor > _previousVersion.minor)
                    {
                        return "Minor Update";
                    }
                    else
                    {
                        if (patch > _previousVersion.patch)
                        {
                            return "Patch";
                        }
                    }
                }
            }
            return "Undefined";
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

    public enum LogType
    {
        Bugfix,
        Addition,
        Change,
        Undefined
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
            CheckForUpdates();
            CheckForChangeLog();
        }

        private void Update_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Process.Start(Directory.GetCurrentDirectory() + "\\UpdateHelper.exe", string.Empty);
            MainWindow.Instance.Close();
        }

        private void Read_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ListBoxItem listBoxItem = button.DataContext as ListBoxItem;

            foreach (var item in changelogListBox.Items)
            {
                if (item == listBoxItem.Content)
                {
                    changelogListBox.SelectedItem = item;
                }
            }

            ChangeLogViewerDialog changeLogViewer = new();
            changeLogViewer.Owner = MainWindow.Instance;
            changeLogViewer.updateType.Text = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].UpdateType;
            changeLogViewer.changelogTitle.Text = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Name;
            changeLogViewer.rarityBorder.BorderBrush = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].RarityBorder;
            changeLogViewer.importantUpdateSymbol.Visibility = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].ImportantUpdate;
            changeLogViewer.importantUpdateSymbol.ToolTip = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].ImportantUpdateReason;
            changeLogViewer.bugfixList.Document.Blocks.Clear();
            changeLogViewer.bugfixList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].BugFixes)));
            changeLogViewer.additionList.Document.Blocks.Clear();
            changeLogViewer.additionList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Additions)));
            changeLogViewer.changeList.Document.Blocks.Clear();
            changeLogViewer.changeList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Changes)));
            changeLogViewer.ShowDialog();
        }

        private void OnListBoxItem_Mouse_DoubleClick(object sender, RoutedEventArgs e)
        {
            ChangeLogViewerDialog changeLogViewer = new();
            changeLogViewer.Owner = MainWindow.Instance;
            changeLogViewer.updateType.Text = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].UpdateType;
            changeLogViewer.changelogTitle.Text = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Name;
            changeLogViewer.rarityBorder.BorderBrush = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].RarityBorder;
            changeLogViewer.importantUpdateSymbol.Visibility = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].ImportantUpdate;
            changeLogViewer.importantUpdateSymbol.ToolTip = LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].ImportantUpdateReason;
            changeLogViewer.bugfixList.Document.Blocks.Clear();
            changeLogViewer.bugfixList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].BugFixes)));
            changeLogViewer.additionList.Document.Blocks.Clear();
            changeLogViewer.additionList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Additions)));
            changeLogViewer.changeList.Document.Blocks.Clear();
            changeLogViewer.changeList.Document.Blocks.Add(new Paragraph(new Run(LoadChangeLogs.ChangeLogs[changelogListBox.SelectedIndex].Changes)));
            changeLogViewer.ShowDialog();
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
                MainWindow.Instance.updateNotifier.Visibility = Visibility.Visible;
                topText.Text = "New update available!";
                updateButton.IsEnabled = true;
            }
            else
            {
                MainWindow.Instance.updateNotifier.Visibility = Visibility.Hidden;
                topText.Text = "You are on the latest version.";
            }
        }

        private void CheckForChangeLogCallback()
        {
            if (changeLogs != null)
            {
                Version PreviousVersion = new Version(0, 0, 0, 0);

                for (int i = 0; i < changeLogs.Length; i++)
                {
                    string line = changeLogs[i];

                    if (line.StartsWith('>'))
                    {
                        ChangeLog changeLog = new();

                        changeLog.Name = line.Substring(line.IndexOf('>') + 1);

                        var cLVersionString = line.Substring(line.IndexOf('n') + 2);
                        changeLog.VersionNum = new Version(cLVersionString);
                        changeLog.UpdateType = changeLog.VersionNum.GetUpdateType(PreviousVersion);
                        PreviousVersion = changeLog.VersionNum;

                        switch (changeLog.UpdateType)
                        {
                            case "Patch":
                                changeLog.RarityBorder = (GradientBrush)FindResource("Launcher.Rarity.RareGradient");
                                break;
                            case "Minor Update":
                                changeLog.RarityBorder = (GradientBrush)FindResource("Launcher.Rarity.EpicGradient");
                                break;
                            case "Major Update":
                                changeLog.RarityBorder = (GradientBrush)FindResource("Launcher.Rarity.LegendaryGradient");
                                break;
                            case "Release":
                                changeLog.RarityBorder = (GradientBrush)FindResource("Launcher.Rarity.LegendaryGradient");
                                break;
                            default:
                                changeLog.RarityBorder = (GradientBrush)FindResource("Launcher.Rarity.UndefinedGradient");
                                break;
                        }

                        string nameSuffix = string.Empty;
                        List<string> additionLines = new();
                        List<string> changeLines = new();
                        List<string> bugFixLines = new();
                        bool searchSkip = false;
                        bool searchEnd = false;
                        int nextId = i + 1;
                        LogType type = LogType.Undefined;

                        if (changeLogs[nextId].StartsWith('{'))
                        {
                            if (changeLogs[nextId].Length != 1)
                            {
                                nameSuffix = changeLogs[nextId].Substring(changeLogs[nextId].IndexOf('{') + 1);
                                changeLog.Name = changeLog.Name + " - " + nameSuffix;
                            }

                            while (!searchEnd)
                            {
                                nextId++;
                                searchSkip = false;

                                if (string.IsNullOrEmpty(changeLogs[nextId]))
                                {
                                    searchSkip = true;
                                }
                                else if (changeLogs[nextId].StartsWith("Bug Fixes:"))
                                {
                                    type = LogType.Bugfix;
                                    searchSkip = true;
                                }
                                else if (changeLogs[nextId].StartsWith("Additions:"))
                                {
                                    type = LogType.Addition;
                                    searchSkip = true;
                                }
                                else if (changeLogs[nextId].StartsWith("Changes:"))
                                {
                                    type = LogType.Change;
                                    searchSkip = true;
                                }

                                // Skips this line if it is a header line
                                if (!searchSkip)
                                {
                                    // Checks if this line is the end, if not adds the line into it's specific block of the change log
                                    if (!changeLogs[nextId].StartsWith('}'))
                                    {
                                        switch (type)
                                        {
                                            case LogType.Bugfix:
                                                bugFixLines.Add(changeLogs[nextId]);
                                                break;
                                            case LogType.Addition:
                                                additionLines.Add(changeLogs[nextId]);
                                                break;
                                            case LogType.Change:
                                                changeLines.Add(changeLogs[nextId]);
                                                break;
                                            case LogType.Undefined:
                                                changeLines.Add(changeLogs[nextId]);
                                                break;
                                        }
                                    }
                                    else if (changeLogs[nextId].StartsWith("}!"))
                                    {
                                        changeLog.ImportantUpdate = Visibility.Visible;
                                        changeLog.ImportantUpdateReason = changeLogs[nextId].Substring(changeLogs[nextId].IndexOf('!') + 1);
                                        searchEnd = true;
                                    }
                                    else
                                    {
                                        searchEnd = true;
                                    }
                                }
                            }

                            // Adds the changes into the changes block of the change log
                            string changeContent = "";
                            foreach (var change in changeLines)
                            {
                                changeContent += change + "\n";
                            }
                            if (!string.IsNullOrEmpty(changeContent))
                                changeLog.Changes = changeContent;

                            // Adds the additions into the additions block of the change log
                            string additionContent = "";
                            foreach (var addition in additionLines)
                            {
                                additionContent += addition + "\n";
                            }
                            if (!string.IsNullOrEmpty(additionContent))
                                changeLog.Additions = additionContent;

                            // Adds the bug fixes into the bug fix block of the change log
                            string bugFixContent = "";
                            foreach (var bugFix in bugFixLines)
                            {
                                bugFixContent += bugFix + "\n";
                            }
                            if (!string.IsNullOrEmpty(bugFixContent))
                                changeLog.BugFixes = bugFixContent;


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
                    loadingText.Text = "Cannot load change history!";
                }
            }
            else
            {
                loadingText.Text = "Cannot load change history!";
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
                Action ac = () =>
                {
                    latestVersionText.Text = "Latest Version: Unknown";
                    CheckForUpdatesCallback();
                };
                Dispatcher.BeginInvoke(ac);
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
