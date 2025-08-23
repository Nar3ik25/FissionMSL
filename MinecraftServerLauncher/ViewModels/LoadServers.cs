// Made by Kieran Kelly
// Last changed on 2025-08-22 at 20:36
// Yippieeee!

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace MinecraftServerLauncher.ViewModels
{
    /// <summary>
    /// A class that holds all of the info for a server.
    /// </summary>
    public class ServerData
    {
        // Basic data
        public int ID { get; set; }
        public BitmapImage ServerIcon { get; set; }
        public string ServerName { get; set; }
        public string MOTD {  get; set; }
        public string ServerVersion { get; set; }
        public string ServerPath { get; set; }
        public string ServerFilePath { get; set; }
        public string ServerPort { get; set; }
        public string ServerIP { get; set; }
        public string ServerRam { get; set; }
        public string ServerRenderDistance { get; set; }
        public string ServerSeed { get; set; }
        public string ServerGamemode { get; set; }
        public bool ServerHardcore { get; set; }
        public string LastRan { get; set; }
        public DateTime LastUsedDate { get; set; }

        // Compound data
        public string ServerGamemodeFull { get; set; }

        /// <summary>
        /// Finalizes the details of the ServerData class.
        /// </summary>
        public void FinishData()
        {
            if (ServerHardcore)
            {
                ServerGamemodeFull = "Gamemode: Hardcore";
            }
            else if (ServerGamemode == "survival")
            {
                ServerGamemodeFull = "Gamemode: Survival";
            }
            else if (ServerGamemode == "creative")
            {
                ServerGamemodeFull = "Gamemode: Creative";
            }
            else if (ServerGamemode == "adventure")
            {
                ServerGamemodeFull = "Gamemode: Adventure";
            }
            else
            {
                ServerGamemodeFull = "Gamemode: Unknown";
            }

            if (LastUsedDate == DateTime.MinValue)
            {
                LastRan = "Last Started: Never";
            }
            else
            {
                DateTime when = LastUsedDate;
                TimeSpan ts = DateTime.Now.Subtract(when);
                if (ts.TotalMinutes < 1)
                    LastRan = "Last Started: Just now";
                else if (ts.TotalMinutes < 2)
                    LastRan = "Last Started: 1 minute ago";
                else if (ts.TotalHours < 1)
                    LastRan = "Last Started: " + (int)ts.TotalMinutes + " minutes ago";
                else if (ts.TotalHours < 2)
                    LastRan = "Last Started: 1 hour ago";
                else if (ts.TotalDays < 1)
                    LastRan = "Last Started: " + (int)ts.TotalHours + " hours ago";
                else if (ts.TotalDays < 2)
                    LastRan = "Last Started: Yesterday";
                else
                    LastRan = "Last Started: " + (int)ts.TotalDays + " days ago";
            }
        }
    }

    internal class LoadServers
    {
        private static readonly string _applicationDataPath = MainWindow.ApplicationDataPath;
        private static readonly ObservableCollection<ServerData> _servers = new ObservableCollection<ServerData>();
        /// <summary>
        /// List of servers in the server view.
        /// </summary>
        public static ReadOnlyObservableCollection<ServerData> Servers { get; }

        /// <summary>
        /// Refreshes the server listing view from the 'ServerList.txt' file and removes any non-existant servers.
        /// </summary>
        public static void RefreshList()
        {
            try
            {
                List<string> pathLines = new List<string>();

                if (File.Exists(_applicationDataPath + "ServerList.txt"))
                {
                    _servers.Clear();
                    int id = 0;

                    pathLines = File.ReadAllLines(_applicationDataPath + "ServerList.txt").ToList();
                    List<string> lostLines = new List<string>();

                    foreach (string line in pathLines)
                    {
                        if (File.Exists(line))
                        {
                            List<string> lines = File.ReadAllLines(line).ToList();
                            ServerData serverData = new ServerData();

                            // Start of the sorting regexes
                            var nameRegex = new Regex(@"(?:^|\W)name(?:$|\W)");
                            var pathRegex = new Regex(@"(?:^|\W)jar-(?:$|\W)");
                            var filePathRegex = new Regex(@"(?:^|\W)file(?:$|\W)");
                            var ramRegex = new Regex(@"(?:^|\W)ram-(?:$|\W)");
                            var dateRegex = new Regex(@"(?:^|\W)date(?:$|\W)");
                            var seedRegex = new Regex(@"(?:^|\W)seed(?:$|\W)");
                            var motdRegex = new Regex(@"(?:^|\W)motd(?:$|\W)");
                            var portRegex = new Regex(@"(?:^|\W)server-p(?:$|\W)");
                            var ipRegex = new Regex(@"(?:^|\W)server-i(?:$|\W)");
                            var viewDistRegex = new Regex(@"(?:^|\W)view-dis(?:$|\W)");
                            var gamemodeRegex = new Regex(@"(?:^|\W)gamemode(?:$|\W)");
                            var hardcoreRegex = new Regex(@"(?:^|\W)hardcore(?:$|\W)");
                            // End of the sorting regexes

                            // Sorts through all of the lines in the 'FissionMSL.data' file
                            for (int i = 0; i < lines.Count; i++)
                            {
                                var firstFourChars0 = lines[i].Length <= 4 ? lines[i] : lines[i].Substring(0, 4);
                                if (nameRegex.IsMatch(firstFourChars0))
                                {
                                    serverData.ServerName = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                }
                                else if (pathRegex.IsMatch(firstFourChars0))
                                {
                                    serverData.ServerPath = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                }
                                else if (filePathRegex.IsMatch(firstFourChars0))
                                {
                                    serverData.ServerFilePath = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                }
                                else if (ramRegex.IsMatch(firstFourChars0))
                                {
                                    serverData.ServerRam = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                }
                                else if (dateRegex.IsMatch(firstFourChars0))
                                {
                                    var stringDate = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                    if (stringDate == "never")
                                    {
                                        serverData.LastUsedDate = DateTime.MinValue;
                                    }
                                    else
                                    {
                                        serverData.LastUsedDate = DateTime.Parse(stringDate);
                                    }
                                }
                            }

                            // Checks if this is a valid 'FissionMSL.data' file
                            if (!Directory.Exists(serverData.ServerFilePath))
                            {
                                // If not valid, this server will be removed from the 'ServerList.txt' file
                                lostLines.Add(line);
                            }
                            else
                            {
                                // If valid, check the server version file and finalize adding the server to the list view

                                string[] version = Directory.GetFiles(serverData.ServerFilePath, "*.version");

                                if (version.Length == 0)
                                {
                                    serverData.ServerVersion = "Unknown Version";
                                }
                                else
                                {
                                    serverData.ServerVersion = "Minecraft " + Path.GetFileNameWithoutExtension(version[0]);
                                }

                                // Looks at the server.properties file and gets the relevent data.
                                if (File.Exists(serverData.ServerFilePath + "server.properties"))
                                {
                                    string[] propertiesLines = File.ReadAllLines(serverData.ServerFilePath + "server.properties");

                                    for (int i = 0; i < propertiesLines.Length; i++)
                                    {
                                        var firstEightChars = propertiesLines[i].Length <= 8 ? propertiesLines[i] : propertiesLines[i].Substring(0, 8);

                                        if (ipRegex.IsMatch(firstEightChars))
                                        {
                                            serverData.ServerIP = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);
                                        }
                                        else if (portRegex.IsMatch(firstEightChars))
                                        {
                                            serverData.ServerPort = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);
                                        }
                                        else if (gamemodeRegex.IsMatch(firstEightChars))
                                        {
                                            serverData.ServerGamemode = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);
                                        }
                                        else if (viewDistRegex.IsMatch(firstEightChars))
                                        {
                                            serverData.ServerRenderDistance = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);
                                        }
                                        else if (hardcoreRegex.IsMatch(firstEightChars))
                                        {
                                            var boolean = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);
                                            if (boolean == "true")
                                                serverData.ServerHardcore = true;
                                            else
                                                serverData.ServerHardcore = false;
                                        }
                                        else
                                        {
                                            var firstFourChars = propertiesLines[i].Length <= 4 ? propertiesLines[i] : propertiesLines[i].Substring(0, 4);

                                            if (seedRegex.IsMatch(firstFourChars))
                                            {
                                                serverData.ServerSeed = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);
                                            }
                                            else if (motdRegex.IsMatch(firstFourChars))
                                            {
                                                serverData.MOTD = propertiesLines[i].Substring(propertiesLines[i].IndexOf('=') + 1);

                                                if (string.IsNullOrEmpty(serverData.MOTD))
                                                    serverData.MOTD = @"\u00a76\u2550\u2550 \u2605\u00a7b A Cool Minecraft Server \u00a76\u2605 \u2550\u2550\u00a7r\n\u00a77Hosted with\u00a7f Fission\u00a74 MSL";
                                            }
                                        }
                                    }
                                }

                                if (File.Exists(serverData.ServerFilePath + "server-icon.png"))
                                {
                                    Bitmap img = new Bitmap(serverData.ServerFilePath + "server-icon.png");
                                    serverData.ServerIcon = MainWindow.Bitmap2BitmapImage(img);
                                }
                                else
                                {
                                    serverData.ServerIcon = new BitmapImage(new Uri("pack://application:,,,/Dictionaries/ServerMysteryIcon.png", UriKind.Absolute));
                                }

                                serverData.ID = id;
                                id++;

                                serverData.FinishData();

                                _servers.Add(serverData);
                            }
                        }
                        else
                        {
                            // Adds this server to the invalid server list
                            lostLines.Add(line);
                        }
                    }

                    // Remove all invalid servers from the 'ServerList.txt' file
                    foreach (string lostLine in lostLines)
                    {
                        pathLines.Remove(lostLine);
                    }

                    try
                    {
                        if (File.Exists(_applicationDataPath + "ServerList.txt"))
                        {
                            File.WriteAllLines(_applicationDataPath + "ServerList.txt", pathLines);
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    _servers.Clear();
                }
            }
            catch
            {
                // TODO: Probably need an error message for this, not sure yet though.
            }
        }

        // Constructor for LoadServers
        static LoadServers()
        {
            try
            {
                Servers = new ReadOnlyObservableCollection<ServerData>(_servers);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
