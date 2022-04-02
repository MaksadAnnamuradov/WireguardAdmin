using Microsoft.AspNetCore.Mvc;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireguardAdminClient.Models;

namespace WireguardAdminClient.Services
{
    [Headers("Content-Type: application/json")]
    public interface IWireguardService
    {

        [Post("/api/authentication/signup")]
        Task<Response> SignupUser([Body] SignupModelDto signupModel);

        [Post("/api/authentication/login")]
        Task<LoginResponse> LoginUser([Body] Login loginModel);

        [Get("/api/authentication/{scheme}")]
        Task<IActionResult> ExternalLogin(string scheme);

        [Get("/api/profile")]
        [Headers("Authorization: Bearer")]
        Task Profile();
    }
}
