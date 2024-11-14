using AutoMapper;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.DTOs.LossCostDTOs.LossCostTypeDTOs;
using MyAPI.DTOs.PromotionDTOs;
using MyAPI.DTOs.PromotionUserDTOs;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.ReviewDTOs;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.DTOs.VehicleTripDTOs;
using MyAPI.Models;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicleDTOs;
using MyAPI.DTOs.PointUserDTOs;

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
            CreateMap<User, UserPostLoginDTO>().ReverseMap();
            CreateMap<UserPostLoginDTO, User>().ReverseMap();
            CreateMap<User, AccountListDTO>().ReverseMap();
            CreateMap<Role, AccountRoleDTO>().ReverseMap();
            CreateMap<Trip,TripDTO>().ReverseMap();
            CreateMap<TripDTO,Trip>().ReverseMap();
            CreateMap<Trip,TripVehicleDTO>().ReverseMap();
            CreateMap<Trip, DriverTripDTO>();
            CreateMap<Vehicle,VehicleDTO>().ReverseMap();
            CreateMap<Driver,DriverTripDTO>().ReverseMap();
            CreateMap<Driver, UpdateDriverDTO>().ReverseMap();
            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<TypeOfDriver, TypeOfDriverDTO>().ReverseMap();
            CreateMap<TypeOfDriver, UpdateTypeOfDriverDTO>().ReverseMap();
            CreateMap<Request, RequestDTO>().ReverseMap();
            CreateMap<RequestDetail, RequestDetailDTO>().ReverseMap();
            CreateMap<TripDetail, TripDetailsDTO>().ReverseMap();
            CreateMap<TicketDTOs,Ticket>().ReverseMap();
            CreateMap<Ticket, TicketDTOs>().ReverseMap();
            CreateMap<Ticket, ListTicketDTOs>().ReverseMap();
            CreateMap<Promotion, PromotionDTO>().ReverseMap();
            CreateMap<PromotionUser, PromotionUserDTO>().ReverseMap();
            CreateMap<TripDetail, StartPointTripDetails>().ReverseMap();
            CreateMap<TripDetail, EndPointTripDetails>().ReverseMap();
            CreateMap<VehicleType, VehicleTypeDTO>().ReverseMap();
            CreateMap<Vehicle, VehicleListDTO>().ReverseMap();
            CreateMap<Review, ReviewDTO>().ReverseMap();
            CreateMap<VehicleTrip, VehicleTripDTO>().ReverseMap();
            CreateMap<Trip,EndPointDTO>().ReverseMap();
            CreateMap<Trip,StartPointDTO>().ReverseMap();
            CreateMap<Trip,TripImportDTO>().ReverseMap();
            CreateMap<TripImportDTO, Trip>().ReverseMap();
            CreateMap<LossCostType, LossCostTypeListDTO>().ReverseMap();
            CreateMap<LossCost, AddLostCostVehicleDTOs>().ReverseMap();
            CreateMap<LossCost, AddLostCostVehicleDTOs>().ReverseMap();
            CreateMap<LossCostAddDTOs, LossCost>().ReverseMap();
            CreateMap<PointUser, PointUserDTOs>().ReverseMap();
            CreateMap<Ticket, TicketByIdDTOs>().ReverseMap();

        }
    }
}
