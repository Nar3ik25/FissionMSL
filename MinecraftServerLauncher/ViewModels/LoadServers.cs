// Made by Kieran Kelly
// Last changed on 2025-05-01 at 15:47
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
        public string ServerVersion { get; set; }
        public string ServerPath { get; set; }
        public string ServerFilePath { get; set; }
        public string JavaPath { get; set; }
        public string ServerRam { get; set; }
        public string ServerPort { get; set; }
        public string ServerIP { get; set; }
        public string ServerGamemode { get; set; }
        public bool ServerHardcore { get; set; }
        public DateTime Date { get; set; }

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
                        var javaPathRegex = new Regex(@"(?:^|\W)java(?:$|\W)");
                        var dateRegex = new Regex(@"(?:^|\W)date(?:$|\W)");
                        var ramRegex = new Regex(@"(?:^|\W)ram-(?:$|\W)");
                        var portRegex = new Regex(@"(?:^|\W)port(?:$|\W)");
                        var ipRegex = new Regex(@"(?:^|\W)ipv4(?:$|\W)");
                        var gamemodeRegex = new Regex(@"(?:^|\W)game(?:$|\W)");
                        var hardcoreRegex = new Regex(@"(?:^|\W)hard(?:$|\W)");
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
                            else if (javaPathRegex.IsMatch(firstFourChars0))
                            {
                                serverData.JavaPath = lines[i].Substring(lines[i].IndexOf('=') + 1);
                            }
                            else if (dateRegex.IsMatch(firstFourChars0))
                            {
                                var stringDate = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                serverData.Date = DateTime.Parse(stringDate);
                            }
                            else if (ramRegex.IsMatch(firstFourChars0))
                            {
                                serverData.ServerRam = lines[i].Substring(lines[i].IndexOf('=') + 1);
                            }
                            else if (portRegex.IsMatch(firstFourChars0))
                            {
                                serverData.ServerPort = lines[i].Substring(lines[i].IndexOf('=') + 1);
                            }
                            else if (ipRegex.IsMatch(firstFourChars0))
                            {
                                serverData.ServerIP = lines[i].Substring(lines[i].IndexOf('=') + 1);
                            }
                            else if (gamemodeRegex.IsMatch(firstFourChars0))
                            {
                                serverData.ServerGamemode = lines[i].Substring(lines[i].IndexOf('=') + 1);
                            }
                            else if (hardcoreRegex.IsMatch(firstFourChars0))
                            {
                                var boolean = lines[i].Substring(lines[i].IndexOf('=') + 1);
                                if (boolean == "true")
                                    serverData.ServerHardcore = true;
                                else
                                    serverData.ServerHardcore = false;
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

                            if (version[0] == null)
                            {
                                serverData.ServerVersion = "Unknown Version";
                            }
                            else
                            {
                                serverData.ServerVersion = "Minecraft " + Path.GetFileNameWithoutExtension(version[0]);
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
