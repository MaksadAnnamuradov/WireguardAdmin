using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin.Controllers
{

    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly UserManager<WireguardUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly IAdminRepository adminRepository;

        public UserController(UserManager<WireguardUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<UserController> logger, IAdminRepository adminRepository)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            this.adminRepository = adminRepository;
        }

        [HttpGet("getall")]
        //[Authorize(Roles = UserRoles.Admin)]
        public async Task<IEnumerable<WireguardUser>> GetAllUsers()
        {
            _logger.LogDebug("Attempted to get all users");
            return await adminRepository.GetAllUsers();
        }


        [HttpGet]
        public async Task<ActionResult<WireguardUser>> GetUserByID(string userid)
        {
            if (adminRepository.UserExists(userid))
            {
                _logger.LogDebug("Attempted to get user at Id: {Id}", userid);
                return await adminRepository.GetUserAsync(userid);
            }
            else
            {
                _logger.LogError("Attempted to get user at Id: {Id} that does not exist", userid);
                return BadRequest("User id does not exist");
            }
        }


       /* [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] ApiUser user)
        {
            if (ModelState.IsValid)
            {

                await _dataRepository.EditUserAsync(user);
                _logger.LogInformation("{Prefix}: Edited User with Id: {Id}", Prefixes.USER, user.Id);
                return Ok();
            }
            _logger.LogError("{Prefix}: Invalid edited submitted for Id: {Id}", Prefixes.USER, user.Id);
            return BadRequest("User object is not valid");
        }*/

       /* [HttpDelete]
        public async Task<IActionResult> DeleteUser(string userid)
        {
            if (adminRepository.UserExists(userid))
            {
                //loginfo userid of user being deleted
                await adminRepository.DeleteUser(userid);
                //_logger.LogInformation("{Prefix}: Deleted User with Id: {Id}", Prefixes.USER, userid);
                return Ok();
            }
            else
            {
                //_logger.LogError("{Prefix}: Unable to delete User with Id: {Id}, Id does not exist", Prefixes.USER, userid);
                return BadRequest("User id does not exist");
            }

        }*/

    }

}
