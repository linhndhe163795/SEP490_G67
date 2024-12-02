using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class VehicleSeatStatusRepository : GenericRepository<VehicleSeatStatus>, IVehicleSeatStatustRepository
    {
        public VehicleSeatStatusRepository(SEP490_G67Context context) : base(context)
        {
        }
    }
}
