using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class User
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public string AllowedIPRange { get; set; }
        public string ClientPublicKey { get; set; }
        public string ClientPrivateKey { get; set; }
        public string ClientConfigFile { get; set; }
    }
}

/*Client Name
IP address
Date Added
Allowed IP Range
Client public key
Client private key*/