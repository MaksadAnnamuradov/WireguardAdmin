using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class LoginModel {
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public string AllowedIPRange { get; set; }
        public string ClientPublicKey { get; set; }
        public string ClientPrivateKey { get; set; }

        public string ReturnUrl { get; set; } = "/";
    }
}
