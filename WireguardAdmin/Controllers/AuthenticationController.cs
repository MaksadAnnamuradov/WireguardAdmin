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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using WireguardAdminClient.Models;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

namespace WireguardAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    //Logging template:
    //{Prefix}: {Message}, {ObjectsToBeLogged}
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<WireguardUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(UserManager<WireguardUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Route("Signup")]
        public async Task<IActionResult> Register([FromBody] SignupModel model)
        {
            var userExist = await userManager.FindByEmailAsync(model.UserName);

            if (userExist != null)
            {
                _logger.LogWarning("Attempted to register an existing user");
                return StatusCode(StatusCodes.Status500InternalServerError, " User Already Exist");
            }


            WireguardUser user = new WireguardUser
            {
                UserName = model.UserName,
                ProfileDescription = model.ProfileDescription,
                FavoritePet = model.FavoritePet,
                BirthDate = model.BirthDate,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("Server encountered an error registering user: {Username}/{UserId}", user.UserName, user.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to register new user: " + result.Errors);
            }

            _logger.LogInformation("Successfully registered new user under username: {Username}, Id: {UserId}", user.UserName, user.Id);
            return Ok("User Created Successfully");
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Userame);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSiginKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSiginKey, SecurityAlgorithms.HmacSha256Signature)
                    );
                _logger.LogInformation("Successfully Logged in User: {userId}", user.Id);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    ValidTo = token.ValidTo.ToString("yyyy-MM-ddThh:mm:ss"),
                    User = user
                });
            }
            return Unauthorized();
        }

        /*[HttpGet]
        [Route("{Scheme}")]
        public async Task Get([FromRoute] string scheme)
        {
            const string callbackScheme = "xamarinessentials";

            var auth = await Request.HttpContext.AuthenticateAsync(scheme);

            if (!auth.Succeeded
                || auth?.Principal == null
                || !auth.Principal.Identities.Any(id => id.IsAuthenticated)
                || string.IsNullOrEmpty(auth.Properties.GetTokenValue("access_token")))
            {
                // Not authenticated, challenge
                await Request.HttpContext.ChallengeAsync(scheme);
            }
            else
            {
                var claims = auth.Principal.Identities.FirstOrDefault()?.Claims;



                var username = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
                var id = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var firstname = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value;
                var lastname = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Surname)?.Value;
                var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;


                var userExist = await userManager.FindByEmailAsync(email);

                if (userExist == null)
                {
                    ApiUser user = new ApiUser
                    {
                        Id = id,
                        Email = email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = username.Replace(" ", "").ToLower()
                    };

                    var result = await userManager.CreateAsync(user);

                    if (!result.Succeeded)
                    {
                        Console.WriteLine(result);
                    }

                }



                // Get parameters to send back to the callback
                var qs = new Dictionary<string, string>
                {
                    { "access_token", auth.Properties.GetTokenValue("access_token") },
                    { "refresh_token", auth.Properties.GetTokenValue("refresh_token") ?? string.Empty },
                    { "expires", (auth.Properties.ExpiresUtc?.ToUnixTimeSeconds() ?? -1).ToString() },
                    { "email", email },
                    {"id", id }
                };

                // Build the result url
                var url = callbackScheme + "://#" + string.Join(
                    "&",
                    qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
                    .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // Redirect to final url
                Request.HttpContext.Response.Redirect(url);
            }

        }*/

    }
}
