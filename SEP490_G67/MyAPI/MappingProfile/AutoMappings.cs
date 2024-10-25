using AutoMapper;
using MyAPI.DTOs.AccountDTOs;
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
<<<<<<< Updated upstream
=======
            CreateMap<Trip,TripDTO>().ReverseMap();
            CreateMap<TripDTO,Trip>().ReverseMap();
            CreateMap<Trip,TripVehicleDTO>().ReverseMap();
            CreateMap<Vehicle,VehicleDTO>().ReverseMap();
            CreateMap<Driver,DriverTripDTO>().ReverseMap();
            CreateMap<VehicleType, VehicleTypeDTO>().ReverseMap();
            CreateMap<Vehicle, VehicleListDTO>().ReverseMap();
>>>>>>> Stashed changes
        }
    }
}
