using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using MultiplayerLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data.Linq;
using System.Timers;
using System.Windows.Forms;

namespace Server
{
    class Program
    {
        #region Private members

        private static Server Server;

        private static string ServerIp;

        private static int ServerPort;

        private static string DatabaseFileName;

        private static int MaxAmountOfClients;

        private static string ConfigFileName = "config.txt";

        #endregion

        [STAThread]
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.Write("Do you want to load server settings from config.txt? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                try
                {
                    string[] configInfo = File.ReadAllLines(ConfigFileName);
                    ServerIp = configInfo[0];
                    ServerPort = int.Parse(configInfo[1]);
                    DatabaseFileName = Application.StartupPath + configInfo[2];
                    MaxAmountOfClients = int.Parse(configInfo[3]);
                }
                catch (Exception ex)
                {
                    Pause("Reading config error: " + ex.Message);
                    return;
                }
            }
            else
            {
                Console.Write("Enter ip: ");
                ServerIp = Console.ReadLine();
                Console.Write("Enter port: ");
                ServerPort = int.Parse(Console.ReadLine());

                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "database files (*.mdf)|*.mdf";
                openFileDialog.Title = "Open database file...";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DatabaseFileName = openFileDialog.FileName;
                }
                else
                    return;

                Console.Write("Enter max amount of clients: ");
                MaxAmountOfClients = int.Parse(Console.ReadLine());
            }

            Server = new Server(ServerIp, ServerPort);
            Server.ShowMessageEvent += PrintMessage;
            Server.ConnectDatabase(DatabaseFileName);
            Server.Start(MaxAmountOfClients);

            PrintMessage("Type '/help' to see all commands for server", ConsoleColor.Green);

            // cycle for entering commands to server
            while (true)
            {
                string command = Console.ReadLine().ToLower();
                switch (command)
                {
                    case "/help":
                        {
                            PrintMessage(
                                "COMMANDS:\n" +
                                "/clients - show all connected to server clients\n" +
                                "/remove all clients\n" +
                                "/users - show all registered users in database\n" +
                                "/clear db - clear database\n" +
                                "/clear - clear console\n" +
                                "/stop - stop server workout\n" +
                                "/beep - ?"
                                , ConsoleColor.Green);
                            break;
                        }
                    case "/clients":
                        {
                            StringBuilder sb = new StringBuilder($"Connected clients ({Server.Clients.Count}) : \n{{\n");
                            foreach (var client in Server.Clients)
                            {
                                sb.Append("\t" + client + "\n");
                            }
                            sb.Append("}");
                            PrintMessage(sb.ToString(), ConsoleColor.Blue);
                            break;
                        }
                    case "/remove all clients":
                        {
                            while (Server.Clients.Count != 0)
                            {
                                Server.RemoveClient(Server.Clients[0]);
                            }
                            break;
                        }
                    case "/users":
                        {
                            StringBuilder sb = new StringBuilder($"All users in database ({Server.Database.GetTable<User>().Count()}) : \n{{\n\n");

                            List<User> users = Server.Database.GetTable<User>().ToList();
                            foreach (var user in users)
                            {
                                sb.Append($"\tID:{user.Id}\n" +
                                    $"\tUsername:{user.Username}\n" +
                                    $"\tPassword:{user.Password}\n" +
                                    $"\tMatches:{user.Matches}\n" +
                                    $"\tVictories:{user.Victories}\n" +
                                    $"\tDefeats:{user.Defeats}\n\n");
                            }
                            sb.Append("}");
                            PrintMessage(sb.ToString(), ConsoleColor.Blue);
                            break;
                        }
                    case "/clear db":
                        {
                            Server.ClearDatabase();
                            break;
                        }
                    case "/clear":
                        Console.Clear();
                        break;

                    case "/stop":
                        Server.Stop();
                        Server.Database.Dispose();
                        return;
                        break;

                    case "/beep":
                        Console.WriteLine("Beep!\a");
                        break;

                    default:
                        PrintMessage("Undefined command! Type '/help' to see all commands", ConsoleColor.Red);
                        break;
                }
            }
        }

        static void PrintMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Green;
        }

        /// <summary>
        /// Pause and wait for user's input
        /// </summary>
        /// <param name="text">Message text</param>
        static void Pause(string text = "Press any key to continue...")
        {
            Console.Write(text);
            Console.ReadKey(true);
        }
    }
}
