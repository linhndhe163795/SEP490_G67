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
                if (staffId <= 0)
                {
                    throw new ArgumentException("Staff ID must be greater than 0.");
                }

                if (tripId <= 0)
                {
                    throw new ArgumentException("Trip ID must be greater than 0.");
                }

                if (vehicleId == null)
                {
                    throw new ArgumentNullException(nameof(vehicleId), "Vehicle ID list cannot be null.");
                }

                if (!vehicleId.Any())
                {
                    throw new ArgumentException("Vehicle ID list cannot be empty.");
                }

                if (vehicleId.Any(id => id <= 0))
                {
                    throw new ArgumentException("Each Vehicle ID must be greater than 0.");
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
                throw new Exception($"An error occurred while assigning vehicles to the trip: {ex.Message}");
            }
        }



    }
}
