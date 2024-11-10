using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Models;
using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITripRepository : IRepository<Trip>
    {
        Task<List<TripDTO>> GetListTrip();
        Task<List<TripVehicleDTO>> SreachTrip(string startPoint, string endPoint, string time);
        Task AddTrip(TripDTO trip, int? vehicleId, int userId);  
        Task AssgineTripToVehicle(int tripId, List<int> vehicleId);
        Task UpdateTripById(int tripId, TripDTO trip, int userId);
        Task updateStatusTrip(int id, int userId);
        Task confirmAddValidEntryImport(List<Trip> validEntry);
    }
}
