using System;

namespace WireguardAdminClient.Models
{
    public class TokenObject
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set;}
        public string RefreshToken { get; set;}
    }
}
