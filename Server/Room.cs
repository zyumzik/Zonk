using System;
using MultiplayerLibrary;

namespace Server
{
    /// <summary>
    /// Game room which contains 2 players and conducting data exchange
    /// </summary>
    public class Room
    {
        #region Variables

        /// <summary>
        /// Server which contains this room
        /// </summary>
        public Server Server;

        /// <summary>
        /// First player in room
        /// </summary>
        public Client Player1;

        /// <summary>
        /// Second player in room
        /// </summary>
        public Client Player2;

        /// <summary>
        /// Means turn of move. True if player1 and false if player2
        /// </summary>
        private bool Turn = true;

        /// <summary>
        /// Identification key of room
        /// </summary>
        public int Key;

        /// <summary>
        /// Random variable for generating room keys
        /// </summary>
        private static Random Rnd = new Random();

        #endregion

        #region Methods

        public void Start()
        {
            Send(Player1, new Message()
            {
                Text = "/set game room",
                Key = Key,
                PinnedObject = Player2.Account.Username
            });

            Send(Player2, new Message()
            {
                Text = "/set game room",
                Key = Key,
                PinnedObject = Player1.Account.Username
            });

            Send(Player1, new Message()
            {
                Text = "/change turn"
            });
        }

        /// <summary>
        /// Sends message to user in thread
        /// </summary>
        private void Send(Client receiver, Message message)
        {
            Server.Send(receiver, message);
        }

        /// <summary>
        /// Synchronizes player data between players
        /// </summary>
        public void Sync()
        {
            // sending game data to first player
            Send(Player1, new Message()
            {
                Text = "/sync",
                Key = Key,
                PinnedObject = Player2.PlayerData
            });

            // sending game data to second player
            Send(Player2, new Message()
            {
                Text = "/sync",
                Key = Key,
                PinnedObject = Player1.PlayerData
            });
        }

        /// <summary>
        /// Changes turn of move in game
        /// </summary>
        public void ChangeTurn()
        {
            Turn = !Turn;
            Message message = new Message()
            {
                Text = "/change turn",
                Key = Key
            };

            switch (Turn)
            {
                case true:
                    Send(Player1, message);
                    break;
                case false:
                    Send(Player2, message);
                    break;
            }

            CheckVictory();
        }

        /// <summary>
        /// Ends current match, defines winner
        /// </summary>
        public void CheckVictory()
        {
            if (Player1.PlayerData == null
                | Player2.PlayerData == null)
                return;

            if (Player1.PlayerData.AllPoints < 5000
                & Player2.PlayerData.AllPoints < 5000)
                return;

            if (Player1.PlayerData.AllPoints > Player2.PlayerData.AllPoints)
            {
                Send(Player1, new Message()
                {
                    Text = "/victory",
                    Key = Key,
                    PinnedObject = Player2.PlayerData
                });
                Player1.Account.Matches++;
                Player1.Account.Victories++;

                Send(Player2, new Message()
                {
                    Text = "/defeat",
                    Key = Key,
                    PinnedObject = Player1.PlayerData
                });
                Player2.Account.Matches++;
                Player2.Account.Defeats++;
            }
            else if (Player2.PlayerData.AllPoints > Player1.PlayerData.AllPoints)
            {
                Send(Player1, new Message()
                {
                    Text = "/defeat",
                    Key = Key,
                    PinnedObject = Player2.PlayerData
                });
                Player1.Account.Matches++;
                Player1.Account.Defeats++;

                Send(Player2, new Message()
                {
                    Text = "/victory",
                    Key = Key,
                    PinnedObject = Player1.PlayerData
                });
                Player2.Account.Matches++;
                Player2.Account.Victories++;
            }

            Server.CloseRoom(this);
            Server.SaveDatabase();
        }

        static public int NewKey()
        {
            return Rnd.Next(1000, 10000);
        }

        #endregion
    }
}