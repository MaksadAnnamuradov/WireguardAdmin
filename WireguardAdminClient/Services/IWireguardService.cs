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
        Task SignupUser([Body] SignupModel signupModel);

        [Post("/api/authentication/login")]
        Task LoginUser([Body] Login loginModel);

        [Get("/api/authentication/google-login")]
        Task ExternalLogin();

        [Get("/api/profile")]
        [Headers("Authorization: Bearer")]
        Task Profile();
    }
}
