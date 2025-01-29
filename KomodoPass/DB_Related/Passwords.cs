using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoPass
{
    [Table("KomodoPasswords")]
    public class KomodoPassword
    {

        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string? Title { get; set; }      

        [Column("password")]
        public byte[]? Password { get; set; }

        [Column("salt")]
        public byte[]? Salt { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("website")]
        public string? Website { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
              
    }
}
