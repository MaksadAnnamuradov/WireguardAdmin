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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("/")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var username = wireguardOptions.Value.username;
                var password = wireguardOptions.Value.password;

                if (loginModel.Name == username && loginModel.Password == password)
                {
                    var output = await getStatus();

                    ViewBag.output = output;

                    List<User> users = adminRepository.Users.ToList();

                    return View("Success", users);
                }

            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }



        public RedirectResult Logout(string returnUrl = "/")
        {
            return Redirect(returnUrl);
        }

        [Route("addnewclient")]
        [HttpGet]
        public IActionResult AddNewClient()
        {
            return View();
        }

        [Route("addnewclient")]
        [HttpPost]
        public async Task<IActionResult> AddNewClient(NewClientModel newClient)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = newClient.Name,
                    DateAdded = DateTime.Now,
                    IPAddress = newClient.IPAddress,
                };

                adminRepository.AddUser(user);

                var output = await getStatus();

                await GenereateNewClientConf(newClient);
               /* await UpdateServerFile(newClient);*/

                ViewBag.output = output;

                List<User> users = adminRepository.Users.ToList();

                return View("Success", users);
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");
        }

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

        public async Task UpdateServerFile(NewClientModel newClient)
        {
            string name = newClient.Name;

            await $@"cd /etc/wireguard && echo ""[Peer]"" >> wg0.conf &&
                  echo ""AllowedIPs = {newClient.IPAddress}"" >> wg0.conf &&
                  echo ""PublicKey = $(cat $HOME/wireguard/{name}/{name}.pub)"" >> wg0.conf".Bash();
        }

        [Route("restart")]
        [HttpGet]
        public async Task<IActionResult> Restart()
        {
            await $"sudo systemctl restart wg-quick@wg0.service".Bash();

            var output = await getStatus();

            ViewBag.output = output;

            List<User> users = adminRepository.Users.ToList();

            return View("Success", users);
        }

        public async Task<string> getStatus()
        {
            var output = await $"systemctl status wg-quick@wg0.service".Bash();
            return output;
        }
    }
}
