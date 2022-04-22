using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireguardAdminClient.Models;

namespace WireguardAdminClient.Services
{
    public class WireguardService : IWireguardService
    {

        private readonly IWireguardService wireguardService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string token = "";


        public WireguardService(IHttpContextAccessor httpContextAccessor)
        {
            wireguardService = RestService.For<IWireguardService>("https://localhost:5001", new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () =>
                    Task.FromResult(token)
            });

            _httpContextAccessor = httpContextAccessor;
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

