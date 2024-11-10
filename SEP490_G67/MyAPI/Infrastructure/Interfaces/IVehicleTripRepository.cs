using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IVehicleTripRepository : IRepository<VehicleTrip>
    {
        void assginVehicleToTrip(int staffId ,List<int> vehicleId, int tripId);
    }
}
