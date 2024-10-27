﻿using AutoMapper;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleDTOs;
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
            CreateMap<Trip,TripDTO>().ReverseMap();
            CreateMap<TripDTO,Trip>().ReverseMap();
            CreateMap<Trip,TripVehicleDTO>().ReverseMap();
            CreateMap<Vehicle,VehicleDTO>().ReverseMap();
            CreateMap<Driver,DriverTripDTO>().ReverseMap();
            CreateMap<Driver, UpdateDriverDTO>().ReverseMap();
            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<TypeOfDriver, TypeOfDriverDTO>().ReverseMap();
            CreateMap<TypeOfDriver, UpdateTypeOfDriverDTO>().ReverseMap();
            CreateMap<TripDetail, TripDetailsDTO>().ReverseMap();
            CreateMap<TicketDTOs,Ticket>().ReverseMap();
            CreateMap<Ticket, TicketDTOs>().ReverseMap();

            //CreateMap<VehicleOwner, VehicleDTO>().ReverseMap();
        }
    }
}
