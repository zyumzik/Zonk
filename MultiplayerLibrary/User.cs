using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace MultiplayerLibrary
{
    [Serializable]
    [Table(Name = "User")]
    public class User
    {
        [Column(Name = "Id", IsDbGenerated = true, IsPrimaryKey = true, DbType = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(Name = "Username")]
        public string Username { get; set; }

        [Column(Name = "Password")]
        public string Password { get; set; }

        [Column(Name = "Matches")]
        public int Matches { get; set; }

        [Column(Name = "Victories")]
        public int Victories { get; set; }

        [Column(Name = "Defeats")]
        public int Defeats { get; set; }
    }
}
