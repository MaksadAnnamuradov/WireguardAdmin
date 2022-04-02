using Microsoft.AspNetCore.Mvc;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireguardAdminClient.Models;

namespace WireguardAdminClient.Services
{
    public class WireguardService: IWireguardService
    {

        private readonly IWireguardService wireguardService;

        public WireguardService()
        {
            wireguardService = RestService.For<IWireguardService>("https://localhost:5001");
        }
        public async Task<Response> SignupUser(SignupModelDto signupModel)
        {
            return await wireguardService.SignupUser(signupModel);
        }
        public async Task<LoginResponse> LoginUser(Login loginModel)
        {
            return await wireguardService.LoginUser(loginModel);
        }
        public async Task<IActionResult> ExternalLogin(string scheme)
        {
            return await wireguardService.ExternalLogin(scheme);
        }
        public async Task Profile()
        {
            await wireguardService.Profile();
        }
    }
}
