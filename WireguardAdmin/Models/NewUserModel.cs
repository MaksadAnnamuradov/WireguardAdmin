using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class NewUserModel
    {
        [Key]
        public string ID { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
