﻿using MyAPI.DTOs.TripDTOs;
using MyAPI.Models;
using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITripRepository : IRepository<Trip>
    {
        Task<List<TripDTO>> GetListTrip();
        Task<List<TripVehicleDTO>> SreachTrip(string startPoint, string endPoint, string time);

    }
}
