using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin.Controllers
{

    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IClientRepository clientRepository;

        public ClientController(IMapper mapper, IClientRepository clientRepository)
        {
            this.mapper = mapper;
            this.clientRepository = clientRepository;
        }
        private string getModelStateErrorMessage() =>
        string.Join(" | ",
            ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
            );

        [HttpPost]
        public async Task<ActionResult<NewUserModelDto>> Add([FromBody] SignupModel newUserData)
        {
            if (!ModelState.IsValid)
                return BadRequest(getModelStateErrorMessage());

            var person = await clientRepository.AddAsync(newUserData.UserName, newUserData.Password);
            return mapper.Map<NewUserModelDto>(person);
        }


    }
}