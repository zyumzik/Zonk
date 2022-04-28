using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.Linq;
using MultiplayerLibrary;

namespace Server
{ 
    /// <summary>
    /// Creates new server for game. Can accept new connections, listen and send data to connected clients
    /// </summary>
    public class Server
    {
        #region Server members

        /// <summary>
        /// TcpListener to accept and listen for Tcp connections
        /// </summary>
        public TcpListener TcpListener;

        /// <summary>
        /// Database connection
        /// </summary>
        public DataContext Database;

        /// <summary>
        /// List with clients who have connected to server
        /// </summary>
        public List<Client> Clients = new List<Client>();

        /// <summary>
        /// List with clients who are searching players for game
        /// </summary>
        public List<Client> ClientsInSearch = new List<Client>();

        /// <summary>
        /// List with rooms which contain clients where they always change game data
        /// </summary>
        public List<Room> GameRooms = new List<Room>();

        /// <summary>
        /// Thread for accepting new tcp clients
        /// </summary>
        private Thread AcceptingClientsThread;

        /// <summary>
        /// Thread for sending messages to clients
        /// </summary>
        private Thread SendThread;

        /// <summary>
        /// Delegate for showing messages event
        /// </summary>
        /// <param name="message">Message for showing</param>
        public delegate void ShowMessage(string message, ConsoleColor color);

        /// <summary>
        /// Event for showing messages
        /// </summary>
        public event ShowMessage ShowMessageEvent;

        #endregion

        #region Server constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Server()
        {
            try
            {
                TcpListener = new TcpListener(IPAddress.Any, 7777);
            }
            catch (Exception ex)
            {
                Notify("Constructor error: " + ex.Message, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Parametrized constructor
        /// </summary>
        /// <param name="ip">Server IP address</param>
        /// <param name="port">Server port</param>
        public Server(string ip, int port)
        {
            try
            {
                TcpListener = new TcpListener(IPAddress.Parse(ip), port);
            }
            catch(Exception ex)
            {
                Notify("Constructor error: " + ex.Message, ConsoleColor.Red);
            }
        }

        #endregion

        #region Basement server funtions

        /// <summary>
        /// Starts accepting new connections and listening incoming data
        /// </summary>
        /// <param name="maxClients">Max amount of connected clients</param>
        public void Start(int maxClients = 32)
        {
            TcpListener.Start();

            SendThread = new Thread(Sending);

            // accepting new connections
            AcceptingClientsThread = new Thread(StartAcceptingConnections);
            AcceptingClientsThread.IsBackground = true;
            AcceptingClientsThread.Start(maxClients);

            Notify("Server started...", ConsoleColor.Green);
        }

        /// <summary>
        /// Starts accepting new connections to server
        /// </summary>
        /// <param name="maxClients">Amount of max connected clients</param>
        private void StartAcceptingConnections(object maxClients)
        {
            int clients = 0;
            try
            {
                while (clients != (int)maxClients)
                {
                    Client client = new Client(this, TcpListener.AcceptTcpClient());
                    Clients.Add(client);
                    client.Start();
                    client.IpEndPoint = client.TcpConnection.Client.RemoteEndPoint;
                    clients++;
                    Notify($"Client connected: " +
                        $"{Clients[Clients.Count - 1].TcpConnection.Client.RemoteEndPoint}",
                        ConsoleColor.Green);
                }
                Notify("Max amount of clients was connected", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Notify("Accepting connections error: " + ex.Message, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Notifies message via event
        /// </summary>
        /// <param name="message">Showing message</param>
        public void Notify(string message, ConsoleColor color)
        {
            ShowMessageEvent?.Invoke(message, color);
        }

        /// <summary>
        /// Connects to existing database with users. Also shows open file dialog for choosing database file
        /// </summary>
        /// <param name="connectionString">Database connectiong string</param>
        public void ConnectDatabase(string filename)
        {
            string connectionString =
            @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename =" +
            filename +
            ";Integrated Security=True;Connect Timeout=30";

            try
            {
                Database = new DataContext(connectionString);
                Notify("Server has successfully connected database", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Notify("Connecting database error: " + ex.Message, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Saves all data about current users to database
        /// </summary>
        public void SaveDatabase()
        {
            Database.SubmitChanges();
        }

        /// <summary>
        /// Clears all database information
        /// </summary>
        public void ClearDatabase()
        {
            try
            {
                Database.ExecuteCommand("delete from [User];");
            }
            catch (Exception ex)
            {
                Notify($"Clearing database error: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Sends message to certain client
        /// </summary>
        /// <param name="threadParameters">Object(ThreadParameters) contains: client which will receive message and message</param>
        public void Send(Client client, Message message)
        {
            ThreadParameters parameters = new ThreadParameters(client, message);
            if (SendThread != null & SendThread.IsAlive)
            {
                SendThread.Join();
            }
            SendThread = new Thread(Sending);
            SendThread.IsBackground = true;
            SendThread.Start(parameters);
        }

        /// <summary>
        /// Action for thread to send message to client
        /// </summary>
        /// <param name="src">Thread parameters</param>
        private void Sending(object src)
        {
            try
            {
                ThreadParameters parameters = src as ThreadParameters;
                MemoryStream stream = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(stream, parameters.Message);
                byte[] buf = stream.ToArray();
                parameters.Client.TcpConnection.GetStream().Write(buf, 0, buf.Length);

                Notify($"Message to {parameters.Client}: {parameters.Message} was send", ConsoleColor.White);
            }
            catch (Exception ex)
            {
                ThreadParameters parameters = src as ThreadParameters;
                Notify($"Sending message to ({parameters.Client}) error: {ex.Message}", ConsoleColor.Red);
                if (parameters.Client.GameRoom != null)
                {
                    CloseRoom(parameters.Client.GameRoom);
                }
                RemoveClient(parameters.Client);
            }
        }

        /// <summary>
        /// Handles message and does needed command
        /// </summary>
        /// <param name="sender">Client who send message</param>
        /// <param name="message">Message for handling</param>
        public void HandleMessage(Client sender, Message message)
        {
            switch (message.Text.ToLower())
            {
                // user is logging in
                case "/log user":
                    {
                        List<string> userInfo = message.PinnedObject as List<string>;
                        User user = ConfirmUserEnter(sender, userInfo[0], userInfo[1]);

                        if (user != null)
                        {
                            Send(sender, new Message()
                            {
                                Text = "/authorized",
                                PinnedObject = user
                            });
                            sender.Account = user;
                        }
                        else
                        {
                            Send(sender, new Message()
                            {
                                Text = "/not authorized"
                            });
                        }

                        break;
                    }

                // user is registering new account
                case "/reg user":
                    {
                        List<string> userInfo = message.PinnedObject as List<string>;
                        
                        if (CheckNameValidity(userInfo[0]))
                        {
                            User user = new User()
                            {
                                Username = userInfo[0],
                                Password = userInfo[1]
                            };
                            Database.GetTable<User>().InsertOnSubmit(user);
                            Database.SubmitChanges();
                            if ((user = ConfirmUserEnter(sender, userInfo[0], userInfo[1])) != null)
                            {
                                Send(sender, new Message()
                                {
                                    Text = "/authorized",
                                    PinnedObject = user
                                });
                                sender.Account = user;
                            }
                            else
                            {
                                Send(sender, new Message()
                                {
                                    Text = "not authorized"
                                });
                            }
                        }
                        else
                        {
                            Send(sender, new Message()
                            {
                                Text = "/invalid name"
                            });
                        }

                        break;
                    }

                // user logged out his account
                case "/log out":
                    {
                        sender.Account = null;
                        break;
                    }

                // user starts searching game
                case "/search game":
                    {
                        ClientsInSearch.Add(sender);
                        CheckReadyPlayers();
                        break;
                    }

                // user stopped searching game
                case "/stop search":
                    {
                        ClientsInSearch.Remove(sender);
                        if (sender.GameRoom != null)
                        {
                            CloseRoom(sender.GameRoom);
                        }
                        break;
                    }

                // user created room
                case "/create room":
                    {
                        foreach (var item in GameRooms)
                        {
                            if (item.Key == message.Key)
                            {
                                Send(sender, new Message()
                                {
                                    Text = "/invalid key"
                                });
                                return;
                            }
                        }

                        Room room = new Room()
                        {
                            Server = this,
                            Player1 = sender,
                            Key = message.Key
                        };
                        sender.GameRoom = room;
                        GameRooms.Add(room);
                        break;
                    }

                // user joins room
                case "/join room":
                    {
                        foreach (var room in GameRooms)
                        {
                            if (room.Key == message.Key)
                            {
                                room.Player2 = sender;
                                sender.GameRoom = room;
                                room.Start();
                                return;
                            }
                        }

                        Send(sender, new Message()
                        {
                            Text = "/room not found"
                        });

                        Thread.Sleep(1000);

                        break;
                    }

                // user disconnected room
                case "/disconnect room":
                    {
                        CloseRoom(sender.GameRoom);
                        break;
                    }

                // user synchronizes data (sends data about him)
                case "/sync":
                    {
                        sender.PlayerData = message.PinnedObject as Player;
                        sender.GameRoom.Sync();
                        break;
                    }

                // user ends move, need to change move turn
                case "/end move":
                    {
                        sender.PlayerData = message.PinnedObject as Player;
                        sender.GameRoom.Sync();
                        sender.GameRoom.ChangeTurn();
                        break;
                    }

                // something gone wrong
                default:
                    {
                        Notify($"Undefined message from client {sender.TcpConnection.Client.RemoteEndPoint}:\n" +
                            $"{message}", ConsoleColor.Red);
                        break;
                    }
            }
        }

        /// <summary>
        /// Removes all disconnected clients from lists
        /// </summary>
        public void RemoveClient(Client client)
        {
            Clients.Remove(client);
            ClientsInSearch.Remove(client);
            Notify($"Client {client} was removed from server", ConsoleColor.Red);

            // removing clients from rooms
            for (int i = 0; i < GameRooms.Count; i++)
            {
                if (GameRooms[i].Player1 == client
                    | GameRooms[i].Player2 == client)
                {
                    CloseRoom(GameRooms[i]);
                }
                
            }
        }

        /// <summary>
        /// Checks ready for game players and creates room for them
        /// </summary>
        public void CheckReadyPlayers()
        {
            if (ClientsInSearch.Count >= 2)
            {
                Room room = new Room()
                {
                    Server = this,
                    Player1 = ClientsInSearch[0],
                    Player2 = ClientsInSearch[1],
                    Key = Room.NewKey()
                };

                ClientsInSearch[0].GameRoom = room;
                ClientsInSearch[1].GameRoom = room;

                ClientsInSearch.RemoveAt(1);
                ClientsInSearch.RemoveAt(0);

                Thread.Sleep(1000);

                GameRooms.Add(room);
                room.Start();
            }
        }

        /// <summary>
        /// Removes room from list and ends game
        /// </summary>
        /// <param name="room">Room for closing</param>
        public void CloseRoom(Room room)
        {
            try
            {
                Send(room.Player1, new Message()
                {
                    Text = "/disconnect room"
                });
                Send(room.Player2, new Message()
                {
                    Text = "/disconnect room"
                });
                room.Player1.GameRoom = room.Player2.GameRoom = null;
                GameRooms.Remove(room);
            }
            catch
            {
                Notify("Room was not closed", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Stops server work
        /// </summary>
        public void Stop()
        {
            try
            {
                TcpListener.Stop();
                AcceptingClientsThread.Abort();
                SendThread.Abort();
                Clients.Clear();
                ClientsInSearch.Clear();
                GameRooms.Clear();
            }
            catch (Exception ex)
            {
                Notify($"Server stopping exception: {ex.Message}", ConsoleColor.White);
            }
        }

        #endregion

        #region Handle message functions

        /// <summary>
            /// Checks login data validity in database
            /// </summary>
            /// <param name="username">Account username (login)</param>
            /// <param name="password">Account password</param>
            /// <returns>True if login data is correct and false if login data is incorrect</returns>
        private User ConfirmUserEnter(Client client, string username, string password)
        {
            List<User> users = Database.GetTable<User>().ToList();

            foreach (var user in users)
            {
                if (user.Username == username)
                {
                    if (user.Password == password)
                    {
                        client.Account = user;
                        return user;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates new account in database
        /// </summary>
        /// <param name="username">New account username (login)</param>
        /// <param name="password">New account password</param>
        /// <returns>True if account was created and false if it was not</returns>
        private bool CheckNameValidity(string username)
        {
            List<User> users = Database.GetTable<User>().ToList();

            foreach (var user in users)
            {
                if (user.Username == username)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
