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
                var response = await wireguardService.LoginUser(loginModel);
                return RedirectToPage("/Account/Index");
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }

        [HttpGet]
        public async Task GoogleLogin(string scheme="Google")
        {
    
            Redirect("https://localhost:5001/api/Authentication/Google");
        }


        public async Task<IActionResult> Index()
        {

            return View();

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
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if (ModelState.IsValid)
            {
                SignupModelDto signupModelDto = new SignupModelDto()
                {
                    Email = signupModel.Email,
                    UserName = signupModel.UserName,
                    ProfileDescription = signupModel.ProfileDescription,
                    BirthDate = signupModel.BirthDate,
                    FavoritePet = signupModel.FavoritePet,
                    Password = signupModel.Password,
                };

                var response = await wireguardService.SignupUser(signupModelDto);
                
                if(response.Status == "Success")
                {
                    return RedirectToAction("Index");
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
