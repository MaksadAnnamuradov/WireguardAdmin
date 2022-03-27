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
using WireguardAdmin.Services;

namespace WireguardAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAdminRepository adminRepository;
        private readonly IWireguardService wireguardService;

        public AccountController(IAdminRepository adminRepository, IWireguardService wireguardService)
        {
            this.adminRepository = adminRepository;
            this.wireguardService = wireguardService;
        }


    

        [HttpGet]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var verified = false;
                var users = await adminRepository.GetAllNewUsers();
                var user = users.Where(x => x.UserName == loginModel.Name).FirstOrDefault();

                if (user != null)
                {
                    verified = wireguardService.Check(user.PasswordHash, loginModel.Password, user.PasswordSalt);
                }

                if (user == null && !verified)
                {
                    //Add logic here to display some message to user
                    return BadRequest("Username or password is incorrect");
                }
            
                return View(loginModel);

            }

            return BadRequest("User object is not valid");

        }



        [HttpPost]
        [Route("Signup")]
        public async Task<IActionResult> Signup([FromBody] NewUserModelDto newUser)
        {
            if (ModelState.IsValid)
            {
                // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                byte[] salt = new byte[128 / 8];

                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }

                // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: newUser.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                //Console.WriteLine($"Hashed: {hashedPassword}");

                var UserSessionId = Guid.NewGuid().ToString();
                var UserSessionExpiration = TimeSpan.FromMinutes(2);

                NewUserModelDbo user = new()
                {
                    ID = Guid.NewGuid().ToString(),
                    UserName = newUser.UserName,
                    PasswordHash = hashedPassword,
                    PasswordSalt = salt,
                    DateAdded = DateTime.Now,
                    SessionId = UserSessionId,
                    SessionExpiration = UserSessionExpiration
                };

                await adminRepository.AddNewUser(user);

                return Ok();
            }

            return BadRequest("User object is not valid");
        }


    }
}
