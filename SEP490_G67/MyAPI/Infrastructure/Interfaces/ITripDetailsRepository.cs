using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITripDetailsRepository : IRepository<TripDetail>
    {
        Task<List<TripDetailsDTO>> TripDetailsByTripId(int TripId);   

        Task<List<StartPointTripDetails>> StartPointTripDetailsById (int TripId);
        Task<List<EndPointTripDetails>> EndPointTripDetailsById (int TripId);

    }
}
