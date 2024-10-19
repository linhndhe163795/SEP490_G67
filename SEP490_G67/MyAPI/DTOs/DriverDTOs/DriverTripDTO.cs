using MyAPI.DTOs.TripDTOs;
namespace MyAPI.DTOs.DriverDTOs
{
    public class DriverTripDTO
    {
        public string? Name { get; set; }
        public List<TripDTO> listTrip { get; set; }
    }
}
