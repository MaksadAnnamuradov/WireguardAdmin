using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin.Services
{
    public class WireguardService : IWireguardService
    {
        public async Task GenereateNewClientConf(NewClientModel newClient)
        {
            string name = newClient.Name;

            await $@"umask 077 && cd /home/wireguard/wireguard && mkdir {name} && cd {name} &&
                  wg genkey > {name}.key &&  wg pubkey<{name}.key > {name}.pub &&
                  echo ""[Interface]"" >> {name}.conf &&
                  echo ""PublicKey = $(cat {name}.pub)"" >> {name}.conf &&
                  echo ""PrivateKey = $(cat {name}.key)"" >> {name}.conf && 
                  echo ""Address = {newClient.IPAddress}"" >> {name}.conf &&
                  echo ""DNS = 8.8.8.8"" >> {name}.conf &&
                  echo ""[Peer]"" >> {name}.conf &&
                  echo ""AllowedIPs = 10.254.0.0/24"" >> {name}.conf &&
                  echo ""Endpoint = 74.207.244.207:51820"" >> {name}.conf &&
                  echo ""AllowedIPs = 0.0.0.0/0"" >> {name}.conf".Bash();
        }

        public async Task<string> GetClientPublicKey(string clientName)
        {
            return await $"cat /home/wireguard/wireguard/{clientName}/{clientName}.pub".Bash();
        }

        public async Task<string> GetClientPrivateKey(string clientName)
        {
            return await $"cat /home/wireguard/wireguard/{clientName}/{clientName}.key".Bash();
        }

        public async Task UpdateServerFile(NewClientModel newClient)
        {
            string name = newClient.Name;

            string clientKey = await GetClientPublicKey(name);

            await $@"
                  sudo systemctl stop wg-quick@wg0.service &&
                  cd /etc/wireguard && echo ""[Peer]"" >> wg0.conf &&
                  echo ""AllowedIPs = {newClient.IPAddress}"" >> wg0.conf &&
                  echo ""PublicKey = {clientKey}"" >> wg0.conf &&
                  sudo systemctl start wg-quick@wg0.service".Bash();


            /* await $"sudo wg set wg0 peer {clientKey} allowed-ips {newClient.IPAddress};".Bash();
             await "sudo systemctl restart wg-quick@wg0.service".Bash();*/
        }

        public async Task Restart()
        {
            await $"sudo systemctl restart wg-quick@wg0.service".Bash();
        }

        public async Task<string> getStatus()
        {
            var output = await $"systemctl status wg-quick@wg0.service".Bash();
            return output;
        }

        public bool Check(string hash, string password, byte[] salt)
        {
            var verifiedHash = false;

            var iterations = Convert.ToInt32(100000);
            var key = Convert.FromBase64String(hash);

            var needsUpgrade = iterations != 100000;

            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              salt,
              iterations,
              HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(32);

                verifiedHash = keyToCheck.SequenceEqual(key);

            }

            return verifiedHash;
        }
    }
}
