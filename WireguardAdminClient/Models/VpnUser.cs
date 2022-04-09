using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdminClient.Models
{
    public class VpnUser
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

/*[Interface]
PrivateKey = 4OvR9caAIy0AjGy8zl + J / bc6JC / zk3o7p8fA25 / WIWs =
Address = 10.200.0.2 / 32
DNS = 8.8.8.8

[Peer]
PublicKey = ys2zO7pf2j + sRCe1 + O59Rsf + MO31qNOSjGXlY +/ yTWw =
AllowedIPs = 0.0.0.0 / 0
Endpoint = 74.207.244.207:51820*/
