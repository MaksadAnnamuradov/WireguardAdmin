using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WireguardAdminClient.Models
{
    public class UserModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string ProfileDescription { get; set; }
        public string FavoritePet { get; set; }
        public UploadFile ProfileImage { get; set; } //Stored as byte array in the database.
    }
}
