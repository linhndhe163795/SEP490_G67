using AutoMapper;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Models;

namespace MyAPI.MappingProfile
{
    public class AutoMappings : Profile
    {
        public AutoMappings() 
        {
            CreateMap<User, UserRegisterDTO>().ReverseMap();
            CreateMap<UserLoginDTO, User>().ReverseMap();
            CreateMap<Role, UserLoginDTO>().ReverseMap();
            CreateMap<User, ForgotPasswordDTO>().ReverseMap();
            CreateMap<User, AccountListDTO>().ReverseMap();
            CreateMap<Role, AccountRoleDTO>().ReverseMap();
            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<Driver, UpdateDriverDTO>().ReverseMap();

        }
    }
}
