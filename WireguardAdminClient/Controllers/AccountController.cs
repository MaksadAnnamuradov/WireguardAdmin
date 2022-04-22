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
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Newtonsoft.Json;

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

        [HttpPost]
        public async Task<IActionResult> Login(Login loginModel)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var response = await wireguardService.LoginUser(loginModel);


                    using (HttpClient httpClient = new HttpClient())
                    {

                        AuthenticationProperties options = new AuthenticationProperties();

                        options.AllowRefresh = true;
                        options.IsPersistent = true;
                        options.ExpiresUtc = response.Expiration;
                        options.IsPersistent = loginModel.RememberLogin;
                        options.RedirectUri = loginModel.ReturnUrl;

                        var claims = new[]
                        {
                            new Claim(ClaimTypes.Name, loginModel.Username),
                            new Claim("AcessToken", string.Format("Bearer {0}", response.Token)),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "cookieAuth");

                        await HttpContext.SignInAsync("cookieAuth", new ClaimsPrincipal(claimsIdentity));
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Invalid name or password");
                    Console.WriteLine(e);
                    return View(loginModel);
                }
            }
            else
            {
                return View(loginModel);

            }

        }

        /* [HttpGet]
         public async Task GoogleLogin(string scheme="Google")
         {

             Redirect("https://localhost:5001/api/Authentication/Google");
         }*/


        public async Task<IActionResult> Index()
        {

            return View();

        }

        public async Task<IActionResult> LogOut()
        {
            //SignOutAsync is Extension method for SignOut
            await HttpContext.SignOutAsync("cookieAuth");
            //Redirect to home page
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
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if (ModelState.IsValid)
            {

                var objfiles = new UploadFile();
                var files = signupModel.ProfileImage;

                if (signupModel.ProfileImage != null)
                {
                    if (files.Length > 0)
                    {
                        //Getting FileName
                        var fileName = Path.GetFileName(files.FileName);
                        //Getting file Extension
                        var fileExtension = Path.GetExtension(fileName);

                        if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".jpeg")
                        {
                            // concatenating  FileName + FileExtension
                            var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

                            objfiles = new UploadFile()
                            {
                                DocumentId = 0,
                                Name = newFileName,
                                FileType = fileExtension,
                                CreatedOn = DateTime.Now
                            };

                            using (var target = new MemoryStream())
                            {
                                files.CopyTo(target);
                                objfiles.DataFiles = target.ToArray();
                            }

                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid file type");
                            return View("Signup");
                        }


                    }
                }

                SignupModelDto signupModelDto = new SignupModelDto()
                {
                    Email = signupModel.Email,
                    UserName = signupModel.UserName,
                    ProfileDescription = signupModel.ProfileDescription,
                    BirthDate = signupModel.BirthDate,
                    FavoritePet = signupModel.FavoritePet,
                    Password = signupModel.Password,
                    ProfileImage = objfiles
                };



                var response = await wireguardService.SignupUser(signupModelDto);

                if (response.Status == "Success")
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid name or password");
                    return View("Signup");
                }
            }

            ModelState.AddModelError("", "Invalid name or password");
            return View("Signup");
        }

        [HttpPost]
        public async Task<IActionResult> AddNewClient(NewClientModel newClient)
        {
            if (ModelState.IsValid)
            {
                /* await GenereateNewClientConf(newClient);
                 await UpdateServerFile(newClient);*/

                VpnUser user = new()
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

        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile files)
        {
            if (files != null)
            {
                if (files.Length > 0)
                {
                    //Getting FileName
                    var fileName = Path.GetFileName(files.FileName);
                    //Getting file Extension
                    var fileExtension = Path.GetExtension(fileName);
                    // concatenating  FileName + FileExtension
                    var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

                    var objfiles = new UploadFile()
                    {
                        DocumentId = 0,
                        Name = newFileName,
                        FileType = fileExtension,
                        CreatedOn = DateTime.Now
                    };

                    using (var target = new MemoryStream())
                    {
                        files.CopyTo(target);
                        objfiles.DataFiles = target.ToArray();
                    }

                }
            }
            return View();
        }



    }
}
