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

       // [Route("/")]
       /* public IActionResult Index()
        {
            return RedirectToAction("Login");
        }*/
        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel objLoginModel = new LoginModel();
            objLoginModel.ReturnUrl = ReturnUrl;

            return View(objLoginModel);
        }


        public bool Check(string hash, string password, byte[] salt)
        {
            var verifiedHash = false;

            var iterations = Convert.ToInt32(100000);
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


                if (user == null && !verified)
                {
                    //Add logic here to display some message to user
                    ViewBag.Message = "Invalid Credential";
                    return View(loginModel);
                }
                else
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier,Convert.ToString(user.ID)),
                        new Claim(ClaimTypes.Name,user.UserName),
                        new Claim(ClaimTypes.Role,"User"),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        //AllowRefresh = <bool>,
                        // Refreshing the authentication session should be allowed.

                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        IsPersistent = loginModel.RememberLogin,
                        // Whether the authentication session is persisted across 
                        // multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the
                        // lifetime of the authentication ticket) or session-based.

                        IssuedUtc = DateTimeOffset.UtcNow,
                        // The time at which the authentication ticket was issued.

                        RedirectUri = loginModel.ReturnUrl
                        // The full path or absolute URI to be used as an http 
                        // redirect response value.
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return LocalRedirect("/Home/ConfidentialData");
                }
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }

        //[Route("/")]
        //[Authorize]
        public async Task<IActionResult> Index()
        {
            List<User> users = await adminRepository.GetAllUsers();

            var output = await getStatus();

            ViewBag.output = output;

            return View(users);
        }

        public async Task<IActionResult> LogOut()
        {
            //SignOutAsync is Extension method for SignOut
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //Redirect to home page
            return LocalRedirect("/");
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
               /* await GenereateNewClientConf(newClient);
                await UpdateServerFile(newClient);*/

                User user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = newClient.Name,
                    DateAdded = DateTime.Now,
                    IPAddress = newClient.IPAddress,
                };

                await adminRepository.AddUser(user);

                return RedirectToAction("Index");
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
