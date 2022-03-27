using Microsoft.AspNetCore.Identity;
using System;

namespace WireguardAdmin.Models
{
    public class WireguardUser : IdentityUser
    {

        public string UserName { get; set; }
        public string ProfileDescription { get; set; }
        public string FavoritePet { get; set; }
        public DateTime BirthDate { get; set; }

    }
}
