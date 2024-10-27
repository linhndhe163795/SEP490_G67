using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class VehicleTripRepository : GenericRepository<VehicleTrip>, IVehicleTripRepository
    {
        public VehicleTripRepository(SEP490_G67Context context) : base(context)
        {
        }

    }
}
