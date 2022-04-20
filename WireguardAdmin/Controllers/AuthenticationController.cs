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
using WireguardAdmin.Models.Users;
using Microsoft.AspNetCore.Authentication.Google;
using Auth0.AspNetCore.Authentication;
using WireguardAdmin.Infrastructure;

namespace WireguardAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<WireguardUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<WireguardUser> _signInManager;
        private readonly JwtTokenCreator _jwtCreator;
        private readonly IConfiguration _configuration;

        public AuthenticationController(
            UserManager<WireguardUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<WireguardUser> signInManager,
            JwtTokenCreator jwtCreator,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtCreator = jwtCreator;
            _configuration = configuration;
        }
        /*  [AllowAnonymous]
          [HttpGet]
          [Route("AuthLogin")]
          public async Task Login()
          {
              var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                  // Indicate here where Auth0 should redirect the user after a login.
                  // Note that the resulting absolute Uri must be added to the
                  // **Allowed Callback URLs** settings for the app.
                  .WithRedirectUri("https://localhost:5003")
                  .Build();

              await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
          }

          [Authorize]
          [HttpGet]
          [Route("AuthLogout")]
          public async Task Logout()
          {
              var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                  // Indicate here where Auth0 should redirect the user after a logout.
                  // Note that the resulting absolute Uri must be added to the
                  // **Allowed Logout URLs** settings for the app.
                  .WithRedirectUri(Url.Action("Index", "Home"))
                  .Build();

              await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
              await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          }*/

        /*[Authorize]
        [HttpGet]
        [Route("Profile")]
        public IActionResult Profile()
        {
            return Ok(new
            {
                Name = User.Identity.Name,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }
*/

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                /* var userRoles = await _userManager.GetRolesAsync(user);

                 var authClaims = new List<Claim>
                 {
                     new Claim(ClaimTypes.Name, user.UserName),
                     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 };

                 foreach (var userRole in userRoles)
                 {
                     authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                 }

                 var token = GetToken(authClaims);

                 return Ok(new
                 {
                     Token = new JwtSecurityTokenHandler().WriteToken(token),
                     Expiration = token.ValidTo
                 });*/

                var token = _jwtCreator.Generate(user.Email, user.Id);

                user.RefreshToken = Guid.NewGuid().ToString();

                await _userManager.UpdateAsync(user);

                /*Response.Cookies.Append("X-Access-Token", token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
                Response.Cookies.Append("X-Username", user.UserName, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
                Response.Cookies.Append("X-Refresh-Token", user.RefreshToken, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });*/

                return Ok(new
                {
                    Token = token,
                    Username = user.UserName,
                    RefreshToken = user.RefreshToken,
                }

                    );

            }

            return Unauthorized();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _userManager.FindByNameAsync(model.UserName);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
                if(model.ProfileImage.FileType != ".png" || model.ProfileImage.FileType != ".jpg" || model.ProfileImage.FileType != ".jpeg")
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid file type for profile image" });
                }

                WireguardUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    ProfileDescription = model.ProfileDescription,
                    BirthDate = model.BirthDate,
                    FavoritePet = model.FavoritePet,
                    ProfileImage = model.ProfileImage
                };

                try
                {
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (!result.Succeeded)
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }

                return Ok(new Response { Status = "Success", Message = "User created successfully!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] SignupModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            WireguardUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [HttpGet]
        [Route("{Scheme}")]
        public async Task Get([FromRoute] string scheme = "Google")
        {

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


                var userExist = await _userManager.FindByEmailAsync(email);

                if (userExist == null)
                {
                    WireguardUser user = new WireguardUser
                    {
                        Id = id,
                        Email = email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = username.Replace(" ", "").ToLower()
                    };

                    var result = await _userManager.CreateAsync(user);

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


                // Redirect to final url
                Request.HttpContext.Response.Redirect("https://localhost:5003");
            }

        }
    }

}

