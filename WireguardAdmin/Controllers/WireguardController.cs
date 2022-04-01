using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace WireguardAdmin.Controllers
{
    public class WireguardController
    {
       /* [Route("api/wireguard")]
        [ApiController]
        public class WireGuardController : ControllerBase
        {
            // GET: api/<WireGuardController>
            [HttpGet]
            public string Get()
            {

                try
                {
                    return showWireguard();
                }
                catch (Exception e)
                {
                    return e.ToString();
                }
            }

            [HttpPost]
            public ClientInformation Post(ClientMessageInfo clientInfo)
            {
                try
                {
                    string privateKey = generatePrivateKey();
                    string publicKey = generatePublicKey(privateKey);
                    string addPeerResult = addNewPeer(publicKey, clientInfo.Id);
                    //string restartWireguardResult = restartWireguard();
                    ClientInformation newClient = new(clientInfo.Id, clientInfo.Name, $"10.200.20.{clientInfo.Id}", System.DateTime.Now.ToString(),
                        "10.200.20.*", publicKey, privateKey);
                    return newClient;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            private string addNewPeer(string publicKey, Id id)
            {
                var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "sudo",
                        Arguments = $"wg set wg0 peer {publicKey} allowed-ips 10.200.20.{id}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("should have done it!");
                if (string.IsNullOrEmpty(error)) { return output; }
                else { return error; }
            }

            private string generatePublicKey(string privateKey)
            {
                var command = $"echo '{privateKey}' | wg pubkey";
                var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = $"bash",
                        Arguments = "-c \"" + command + "\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };


                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error)) { return output; }
                else { return error; }
            }

            private string generatePrivateKey()
            {
                var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "wg",
                        Arguments = $"genkey",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error)) { return output; }
                else { return error; }
            }

            private string showWireguard()
            {
                var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "systemctl",
                        Arguments = "status wg-quick@wg0",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error)) { return output; }
                else { return error; }
            }
            private static string restartWireguard()
            {
                var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "sudo",
                        Arguments = "systemctl restart wg-quick@wg0",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error)) { return output; }
                else { return error; }
            }
        }

        [Route("api/wireguard/restart")]
        [ApiController]
        public class WireguardRestartController : ControllerBase
        {
            // GET: api/wireguard/restart
            [HttpGet]
            public string Get()
            {

                try
                {
                    return restartWireguard();
                }
                catch (Exception e)
                {
                    return e.ToString();
                }
            }

            private static string restartWireguard()
            {
                var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "sudo",
                        Arguments = "systemctl restart wg-quick@wg0",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error)) { return output; }
                else { return error; }
            }
        }*/
    }
}
