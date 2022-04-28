using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerLibrary
{
    [Serializable]
    public class Player
    {
        public int AllPoints { get; set; }
        public int RoundPoints { get; set; }
        public int[] DicesValues { get; set; }
    }
}
