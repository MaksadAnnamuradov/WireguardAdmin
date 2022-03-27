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
        public async Task SignupUser(SignupModel signupModel)
        {
            await wireguardService.SignupUser(signupModel);
        }
        public async Task LoginUser(LoginModel loginModel)
        {
            await wireguardService.LoginUser(loginModel);
        }
    }
}
