using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using MultiplayerLibrary;

namespace Server
{
    public class Client
    {
        #region Public members

        /// <summary>
        /// Server which contains this client 
        /// </summary>
        public Server Server;

        /// <summary>
        /// Client connection for connecting server
        /// </summary>
        public TcpClient TcpConnection;

        /// <summary>
        /// Client's ip address and port
        /// </summary>
        public EndPoint IpEndPoint;

        /// <summary>
        /// Account of user in database
        /// </summary>
        public User Account;

        /// <summary>
        /// Last received message from current client
        /// </summary>
        public Message ListenedMessage;

        /// <summary>
        /// Player's game room
        /// </summary>
        public Room GameRoom;

        /// <summary>
        /// Client's listened player game data
        /// </summary>
        public Player PlayerData;

        #endregion

        #region Private members

        /// <summary>
        /// Thread for listening client
        /// </summary>
        private Thread ListenThread;

        #endregion

        #region Constructors

        /// <summary>
        /// Parametrized constructor
        /// </summary>
        /// <param name="connection">Tcp connection of client</param>
        public Client(Server server, TcpClient connection)
        {
            Server = server;
            TcpConnection = connection;
            ListenedMessage = null;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts work of client, also starts listening
        /// </summary>
        public void Start()
        {
            ListenThread = new Thread(Listening);
            ListenThread.Start();
        }

        public override string ToString()
        {
            try
            {
                if (Account == null)
                    return $"{TcpConnection.Client.RemoteEndPoint}";
                else
                    return $"{Account.Username} ({TcpConnection.Client.RemoteEndPoint})";
            }
            catch
            {
                return "/unknown address/";
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts listening current client for incoming data
        /// </summary>
        /// <param name="delayTime">Delay between listening incoming data</param>
        private void Listening()
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                NetworkStream stream = TcpConnection.GetStream();
                while (true)
                {
                    while (stream.DataAvailable)
                    {
                        ListenedMessage = bf.Deserialize(stream) as Message;
                        Server.HandleMessage(this, ListenedMessage);
                        Server.Notify($"Received message from {this.ToString()}:\n" +
                            $"{ListenedMessage}", ConsoleColor.Yellow);
                    }
                }
            }
            catch (Exception ex)
            {
                Server.Notify($"Listening error {this.ToString()}: {ex.Message}", ConsoleColor.Red);
                Server.RemoveClient(this);
                TcpConnection.Close();
                ListenThread.Abort();
            }
        }

        #endregion
    }
}
