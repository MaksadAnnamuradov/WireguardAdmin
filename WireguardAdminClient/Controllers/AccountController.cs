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
using Microsoft.AspNetCore.Authentication.Google;

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
            Login objLoginModel = new Login();
            objLoginModel.ReturnUrl = ReturnUrl;

            return View(objLoginModel);
        }


        //[Route("google-login")]
        [HttpGet]
        public IActionResult ExternalLogin()
        {
            return new ChallengeResult(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse", "GoogleLogin") // Where google responds back
                });
        }

        /// <summary>
        /// Google Login Response After Login Operation From Google Page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            //Check authentication response as mentioned on startup file as o.DefaultSignInScheme = "External"
            var authenticateResult = await HttpContext.AuthenticateAsync("External");
            if (!authenticateResult.Succeeded)
                return BadRequest(); // TODO: Handle this better.
            //Check if the redirection has been done via google or any other links
            if (authenticateResult.Principal.Identities.ToList()[0].AuthenticationType.ToLower() == "google")
            {
                //check if principal value exists or not 
                if (authenticateResult.Principal != null)
                {
                    //get google account id for any operation to be carried out on the basis of the id
                    var googleAccountId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    //claim value initialization as mentioned on the startup file with o.DefaultScheme = "Application"
                    var claimsIdentity = new ClaimsIdentity("Application");
                    if (authenticateResult.Principal != null)
                    {
                        //Now add the values on claim and redirect to the page to be accessed after successful login
                        var details = authenticateResult.Principal.Claims.ToList();
                        claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier));// Full Name Of The User
                        claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Email)); // Email Address of The User
                        await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }
            return RedirectToAction("Account", "AddNewClient");
        }


        /* [HttpGet]
         public async Task<IActionResult> ExternalLogin()
         {
             await wireguardService.ExternalLogin();
             return View();
         }*/

        [HttpPost]
        public async Task<IActionResult> Login(Login loginModel)
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
