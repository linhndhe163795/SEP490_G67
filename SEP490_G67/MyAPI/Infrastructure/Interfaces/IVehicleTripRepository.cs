using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IVehicleTripRepository : IRepository<VehicleTrip>
    {
        Task assginVehicleToTrip(int staffId ,List<int> vehicleId, int tripId);
        //Task addVehiceleToTrip(List<int> vehicleId, List<int> tripId);
    }
}
