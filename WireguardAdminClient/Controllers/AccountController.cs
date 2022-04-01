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
using Auth0.AspNetCore.Authentication;

namespace WireguardAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWireguardService wireguardService;

        public AccountController(IWireguardService wireguardService)
        {
            this.wireguardService = wireguardService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            Login objLoginModel = new Login();

            return View(objLoginModel);
        }


        [HttpPost]
        public async Task Login(string returnUrl = "/secure")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
              // Indicate here where Auth0 should redirect the user after a login.
              // Note that the resulting absolute Uri must be added to the
              // **Allowed Callback URLs** settings for the app.
              .WithRedirectUri(returnUrl)
              .Build();

            await HttpContext.ChallengeAsync(
                Auth0Constants.AuthenticationScheme,
                authenticationProperties
              );
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
              // Indicate here where Auth0 should redirect the user after a logout.
              // Note that the resulting absolute Uri must be added to the
              // **Allowed Logout URLs** settings for the app.
              .WithRedirectUri("/")
              .Build();

            // Logout from Auth0
            await HttpContext.SignOutAsync(
              Auth0Constants.AuthenticationScheme,
              authenticationProperties
            );
            // Logout from the application
            await HttpContext.SignOutAsync(
              CookieAuthenticationDefaults.AuthenticationScheme
            );
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
