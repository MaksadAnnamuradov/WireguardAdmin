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

        [Post("/api/account/signup")]
        Task SignupUser([Body] SignupModel signupModel);

        [Post("/api/acount/login")]
        Task LoginUser([Body] Login loginModel);
    }
}
