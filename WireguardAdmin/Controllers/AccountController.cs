using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAdminRepository adminRepository;
        private readonly IOptions<WireguardAdminOptions> wireguardOptions;

        public AccountController(ILogger<AccountController> logger, IAdminRepository adminRepository, IOptions<WireguardAdminOptions> wireguardOptions)
        {
            _logger = logger;
            this.adminRepository = adminRepository;
            this.wireguardOptions = wireguardOptions;
        }

        [Route("/")]
        public IActionResult Account()
        {
            return RedirectToAction("Login");
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var username = wireguardOptions.Value.username;
                var password = wireguardOptions.Value.password;

                if (loginModel.Name == username && loginModel.Password == password)
                {
                    return RedirectToAction("Success");
                }
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }

        public async Task<IActionResult> Success()
        {
            List<User> users = await adminRepository.GetAllUsers();

            var output = getStatus();

            ViewBag.output = output;

            return View(users);
        }

        public RedirectResult Logout(string returnUrl = "/")
        {
            return Redirect(returnUrl);
        }

        [HttpGet]
        public IActionResult AddNewClient()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddNewClient(NewClientModel newClient)
        {
            if (ModelState.IsValid)
            {
                 GenereateNewClientConf(newClient);
                 UpdateServerFile(newClient);

                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = newClient.Name,
                    DateAdded = DateTime.Now,
                    IPAddress = newClient.IPAddress,
                };

                adminRepository.AddUser(user);

                return RedirectToAction("Success");
            }

            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");
        }

        public void GenereateNewClientConf(NewClientModel newClient)
        {
            string name = newClient.Name;

            var output = RunCommand($@"umask 077 && cd /home/wireguard/wireguard && mkdir {name} && cd {name} &&
                                      wg genkey > {name}.key &&  wg pubkey<{name}.key > {name}.pub &&
                                      echo ""[Interface]"" >> {name}.conf &&
                                      echo ""PublicKey = $(cat {name}.pub)"" >> {name}.conf &&
                                      echo ""PrivateKey = $(cat {name}.key)"" >> {name}.conf && 
                                      echo ""Address = {newClient.IPAddress}"" >> {name}.conf &&
                                      echo ""DNS = 8.8.8.8"" >> {name}.conf &&
                                      echo ""[Peer]"" >> {name}.conf &&
                                      echo ""AllowedIPs = 10.254.0.0/24"" >> {name}.conf &&
                                      echo ""Endpoint = 74.207.244.207:51820"" >> {name}.conf &&
                                      echo ""AllowedIPs = 0.0.0.0/0"" >> {name}.conf");
        }

        public string GetClientPublicKey(string clientName)
        {
            return RunCommand($"cat /home/wireguard/wireguard/{clientName}/{clientName}.pub");
        }

        public string GetClientPrivateKey(string clientName)
        {
            return RunCommand($"cat /home/wireguard/wireguard/{clientName}/{clientName}.key");
        }

        public void UpdateServerFile(NewClientModel newClient)
        {
            string name = newClient.Name;

            string clientKey = GetClientPublicKey(name);

            RunCommand($@"sudo wg set wg0 peer{clientKey} allowed-ips {newClient.IPAddress} &&
                   sudo systemctl restart wg-quick@wg0.service");
        }

        [HttpGet]
        public IActionResult Restart()
        {
            RunCommand($"sudo systemctl restart wg-quick@wg0.service");
            return RedirectToAction("Success");
        }

        public string getStatus()
        {
            var output = RunCommand($"systemctl status wg-quick@wg0.service");
            return output;
        }

        string RunCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{command}\"",
                    UserName = "wireguard",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
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
}
