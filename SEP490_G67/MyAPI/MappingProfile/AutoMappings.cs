using AutoMapper;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Models;

namespace MyAPI.MappingProfile
{
    public class AutoMappings : Profile
    {
        public AutoMappings() 
        {
            CreateMap<User, UserRegisterDTO>().ReverseMap();
            CreateMap<User, ForgotPasswordDTO>().ReverseMap();
        }
    }
}
