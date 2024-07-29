using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace BasicAuthApp.Models
{
    [SQLite.Table("Usuarios")]
    public class Usuarios
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Unique]
        public string usuario { get; set; }
        public string password { get; set; }

        
    }

    public class UserResponse
    {
        public User user { get; set; }
        
    }

    public class User
    {
        public int id { get; set; }
       
        public string username { get; set; }
        
        public string password { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }


}

