using AutoMapper;
using WireguardAdmin.Models;

namespace WireguardAdmin.Mappers
{

    public class WireguardAdminMapper : Profile
    {

        public WireguardAdminMapper()
        {

            CreateMap<NewUserModelDbo, NewUserModel>()
                .ReverseMap();

            CreateMap<NewUserModel, NewUserModelDto>()
                .ReverseMap();
        }
    }
}