using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WireguardAdminClient.Models
{
    public class SignupModelDto
    {
      
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string ProfileDescription { get; set; }
        public string FavoritePet { get; set; }
        public UploadFile ProfileImage { get; set; }
    }
}
