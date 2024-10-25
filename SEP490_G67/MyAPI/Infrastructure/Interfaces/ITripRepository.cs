using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Models;
using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITripRepository : IRepository<Trip>
    {
        Task<List<DriverTripDTO>> GetListTrip();
        Task<List<TripVehicleDTO>> SreachTrip(string startPoint, string endPoint, string time);
        Task AddTrip(TripDTO trip);  
        Task AssgineTripToVehicle(int tripId, List<int> vehicleId);
    }
}
