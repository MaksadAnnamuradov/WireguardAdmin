using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAdminRepository adminRepository;

        public AccountController(ILogger<AccountController> logger, IAdminRepository adminRepository)
        {
            _logger = logger;
            this.adminRepository = adminRepository;
        }

        [Route("")]
        [Route("index")]
        [Route("~/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = loginModel.Name,
                    AllowedIPRange = loginModel.AllowedIPRange,
                    DateAdded = DateTime.Now,
                    IPAddress = loginModel.IPAddress,
                    ClientPrivateKey = loginModel.ClientPrivateKey,
                    ClientPublicKey = loginModel.ClientPublicKey
                };

                adminRepository.AddUser(user);

                var output = await Runcmd();

                ViewBag.output = output;

                List<User> users = adminRepository.Users.ToList();

                return View("Success", users);

            }
            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");

        }

        public async Task Restart()
        {
            var output = await $"sudo systemctl restart wg-quick@wg0.service".Bash();
        }

        public async Task<string> Runcmd()
        {
            var output = await $"sudo systemctl status wg-quick@wg0.service".Bash();
            return output;
        }
    }
}
