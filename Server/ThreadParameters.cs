using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiplayerLibrary;
using System.Net.Sockets;

namespace Server
{
    public class ThreadParameters
    { 
        /// <summary>
        /// Tcp connection which receives data
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Message which will be send
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// Parametrized constructor
        /// </summary>
        /// <param name="receiver">Client which receives data</param>
        /// <param name="message">Message which will be send</param>
        public ThreadParameters(Client receiver, Message message)
        {
            Client = receiver;
            Message = message;
        }
    }
}
