using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class VehicleTripRepository : GenericRepository<VehicleTrip>, IVehicleTripRepository
    {
        public VehicleTripRepository(SEP490_G67Context context) : base(context)
        {
        }

        public Task addVehiceleToTrip(IFormFile tripData)
        {
          throw new NotImplementedException();
        }

        public async Task assginVehicleToTrip(int staffId, List<int> vehicleId, int tripId)
        {
            try
            {
                if (vehicleId == null)
                {
                    throw new NullReferenceException("Không có xe nào hợp lệ");
                }
                List<VehicleTrip> vehicleTrip = new List<VehicleTrip>();
                for (int i = 0; i < vehicleId.Count; i++)
                {
                    VehicleTrip vht = new VehicleTrip
                    {
                        TripId = tripId,
                        VehicleId = vehicleId[i],
                        CreatedAt = DateTime.Now,
                        CreatedBy = staffId
                    };
                    vehicleTrip.Add(vht);
                }
                await _context.AddRangeAsync(vehicleTrip);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
