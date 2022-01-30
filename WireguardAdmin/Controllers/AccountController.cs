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
                    /* var output = await Runcmd();

                     ViewBag.output = output;*/

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
        public IActionResult AddNewClient(NewClientModel newClient)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = newClient.Name,
                    AllowedIPRange = newClient.AllowedIPRange,
                    DateAdded = DateTime.Now,
                    IPAddress = newClient.IPAddress,
                    ClientPrivateKey = newClient.ClientPrivateKey,
                    ClientPublicKey = newClient.ClientPublicKey
                };

                adminRepository.AddUser(user);

                /* var output = await Runcmd();

                    ViewBag.output = output;*/

                List<User> users = adminRepository.Users.ToList();

                return View("Success", users);
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");
        }

        [Route("restart")]
        [HttpGet]
        public async Task<IActionResult> Restart()
        {
            await $"sudo systemctl restart wg-quick@wg0.service".Bash();

            var output = await Runcmd();

            ViewBag.output = output;

            List<User> users = adminRepository.Users.ToList();

            return View("Success", users);
        }

        public async Task<string> Runcmd()
        {
            var output = await $"systemctl status wg-quick@wg0.service".Bash();
            return output;
        }
    }
}
