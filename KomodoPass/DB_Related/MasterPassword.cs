using SQLite;
using System;

namespace KomodoPass
{
    [Table("MasterPassword")]
    public class MasterPassword
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string? Username { get; set; } 

        [Column("password_hash")]
        public byte[]? PasswordHash { get; set; } 

        [Column("salt")]
        public byte[]?  Salt { get; set; }  

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;  
       
    }
}
