using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerLibrary
{
    [Serializable]
    public class Message
    {
        /// <summary>
        /// Text of message. Can contain different commands for server or client
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Key of client's room
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// Object for sharing additional data
        /// </summary>
        public object PinnedObject { get; set; }

        public override string ToString()
        {
            return $"T:[{Text}], Obj:[{PinnedObject}]";
        }
    }
}
