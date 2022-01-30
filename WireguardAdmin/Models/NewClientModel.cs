using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class NewClientModel {
        [Required]
        public string Name { get; set; }
        [Required]
        public string IPAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public string AllowedIPRange { get; set; }
        [Required]
        public string ClientPublicKey { get; set; }
        [Required]
        public string ClientPrivateKey { get; set; }
    }
}
