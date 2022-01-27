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
        public IActionResult Login(LoginModel loginModel )
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

                List<User> users = adminRepository.Users.ToList();

                return View("Success", users);
                
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");

        }

        [Route("logout")]
        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        [Route("runcmd")]
        [HttpGet]
        public async Task<int> Runcmd()
        {
            var output = await $"./Scripts/wireguardAdmin.sh".Bash(_logger);
            return output;
        }
    }
}
