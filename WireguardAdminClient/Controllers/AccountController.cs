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
using WireguardAdminClient.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using WireguardAdminClient.Services;

namespace WireguardAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWireguardService wireguardService;

        public AccountController(IWireguardService wireguardService)
        {
            this.wireguardService = wireguardService;
        }


        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel objLoginModel = new LoginModel();
            objLoginModel.ReturnUrl = ReturnUrl;

            return View(objLoginModel);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {

                return RedirectToAction("Index");

            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }


        public async Task<IActionResult> Index()
        {

           return RedirectToAction("Signup");

        }

        public async Task<IActionResult> LogOut()
        {
            return LocalRedirect("/Account/Login");
        }

        [HttpGet]
        public IActionResult AddNewClient()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewClient(NewClientModel newClient)
        {
            if (ModelState.IsValid)
            {
                /* await GenereateNewClientConf(newClient);
                 await UpdateServerFile(newClient);*/

                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = newClient.Name,
                    DateAdded = DateTime.Now,
                    IPAddress = newClient.IPAddress,
                };

                //await adminRepository.AddUser(user);

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");
        }



        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel newUser)
        {
            if (ModelState.IsValid)
            {
                await this.wireguardService.SignupUser(newUser);

                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Invalid name or password");
            return View("Signup");
        }

    }
}
