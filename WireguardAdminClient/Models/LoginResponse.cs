namespace WireguardAdminClient.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public string Username { get; set; }

        public string RefreshToken { get; set; }
    }
}
