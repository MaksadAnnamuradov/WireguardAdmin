﻿using Microsoft.AspNetCore.Authorization;
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
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

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
        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel objLoginModel = new LoginModel();
            objLoginModel.ReturnUrl = ReturnUrl;

            return View(objLoginModel);
        }

        public bool IsValidPassword(string password, string hashPass)
        {
            bool result = true;

            byte[] hashBytes = Convert.FromBase64String(hashPass);
            byte[] salt = new byte[16];

            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);

            byte[] hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 16; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    throw new UnauthorizedAccessException();
                }
            }

            return result;
        }

        public bool Check(string hash, string password, byte[] salt)
        {
            var verifiedHash = false;

            //var parts = hash.Split('.', 3);

           /* if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. " +
                  "Should be formatted as `{iterations}.{salt}.{hash}`");
            }*/

            var iterations = Convert.ToInt32(100000);
/*            var salt = Convert.FromBase64String(salt);*/
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

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var users = await adminRepository.GetAllNewUsers();
                var user = users.Where(x => x.UserName == loginModel.Name).FirstOrDefault();

                var verified = Check(user.PasswordHash, loginModel.Password, user.PasswordSalt);

                //var validPassword = IsValidPassword(loginModel.Password, user.PasswordHash);

                if (user == null && !verified)
                {
                    //Add logic here to display some message to user
                    ViewBag.Message = "Invalid Credential";
                    return View(loginModel);
                }
                else
                {
                    //A claim is a statement about a subject by an issuer and
                    //represent attributes of the subject that are useful in the context of authentication and authorization operations.
                    var claims = new List<Claim>() {
                    new Claim(ClaimTypes.NameIdentifier,Convert.ToString(user.ID)),
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim("FavoriteDrink","Tea")
                    };
                    //Initialize a new instance of the ClaimsIdentity with the claims and authentication scheme
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    //Initialize a new instance of the ClaimsPrincipal with ClaimsIdentity
                    var principal = new ClaimsPrincipal(identity);
                    //SignInAsync is a Extension method for Sign in a principal for the specified scheme.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        principal, new AuthenticationProperties() { IsPersistent = loginModel.RememberLogin });

                    return LocalRedirect(loginModel.ReturnUrl);
                }
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }

        public async Task<IActionResult> Success()
        {
            List<User> users = await adminRepository.GetAllUsers();

           /* var output = await getStatus();

            ViewBag.output = output;*/

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

        [HttpGet]
        public IActionResult AddNewUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewClient(NewClientModel newClient)
        {
            if (ModelState.IsValid)
            {
                await GenereateNewClientConf(newClient);
                await UpdateServerFile(newClient);

                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = newClient.Name,
                    DateAdded = DateTime.Now,
                    IPAddress = newClient.IPAddress,
                };

                await adminRepository.AddUser(user);

                return RedirectToAction("Success");
            }

            ModelState.AddModelError("", "Invalid name or password");
            return View("Index");
        }



        [HttpPost]
        public async Task<IActionResult> AddNewUser(NewUserModelDto newUser)
        {
            if (ModelState.IsValid)
            {
                // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                byte[] salt = new byte[128 / 8];

                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }

                //Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

                // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: newUser.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                //Console.WriteLine($"Hashed: {hashedPassword}");

                NewUserModel user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    UserName = newUser.UserName,
                    PasswordHash = hashedPassword,
                    PasswordSalt = salt,
                    DateAdded = DateTime.Now,
                };

                await adminRepository.AddNewUser(user);

                return RedirectToAction("Login");
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

        [HttpGet]
        public async Task<IActionResult> Restart()
        {
            await $"sudo systemctl restart wg-quick@wg0.service".Bash();
            return RedirectToAction("Success");
        }

        public async Task<string> getStatus()
        {
            var output = await $"systemctl status wg-quick@wg0.service".Bash();
            return output;
        }
    }
}
