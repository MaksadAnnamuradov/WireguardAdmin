using AutoMapper;
using WireguardAdmin.Models;

namespace WireguardAdmin.Mappers
{

    public class AspenMapperProfile : Profile
    {

        public AspenMapperProfile()
        {

            CreateMap<NewUserModelDbo, NewUserModel>()
                .ReverseMap();

            CreateMap<NewUserModel, NewUserModelDto>()
                .ReverseMap();
        }
    }
}