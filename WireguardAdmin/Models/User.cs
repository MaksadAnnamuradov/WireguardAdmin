using System.Collections.Generic;
using System;

namespace WireguardAdmin.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public string AllowedIPRange { get; set; }
        public string ClientPublicKey { get; set; }
        public string ClientPrivateKey { get; set; }
    }
}

/*Client Name
IP address
Date Added
Allowed IP Range
Client public key
Client private key*/